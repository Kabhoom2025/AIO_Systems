using System.Text.Json;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LinkShield.Infrastructure.ThreatIntelligence;

/// <summary>
/// Queries every enabled provider that has a registered adapter, in parallel, caching each
/// provider's result in Redis (TTL from that provider's row) so repeated scans of the same URL
/// don't re-hit rate-limited external APIs. Persists one ThreatIntelligenceResult row per
/// provider per scan regardless of cache hit/miss, so history is always complete.
/// </summary>
public class ThreatIntelligenceService : IThreatIntelligenceService
{
    private readonly LinkShieldDbContext _db;
    private readonly IReadOnlyDictionary<string, IThreatIntelligenceProvider> _providersBySlug;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ThreatIntelligenceService> _logger;

    public ThreatIntelligenceService(
        LinkShieldDbContext db,
        IEnumerable<IThreatIntelligenceProvider> providers,
        IDistributedCache cache,
        ILogger<ThreatIntelligenceService> logger)
    {
        _db = db;
        _providersBySlug = providers.ToDictionary(p => p.ProviderSlug);
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ThreatIntelligenceResultDto>> CheckAllAsync(
        Guid urlScanId, string url, string domain, CancellationToken ct = default)
    {
        var enabledProviders = await _db.ThreatIntelligenceProviders
            .Where(p => p.IsEnabled)
            .ToListAsync(ct);

        var tasks = enabledProviders
            .Where(p => _providersBySlug.ContainsKey(p.Slug))
            .Select(p => CheckOneAsync(p, url, domain, ct));

        var results = await Task.WhenAll(tasks);

        foreach (var (provider, result, fromCache) in results)
        {
            _db.ThreatIntelligenceResults.Add(new ThreatIntelligenceResult
            {
                UrlScanId = urlScanId,
                ThreatIntelligenceProviderId = provider.Id,
                QueriedValue = url,
                Status = result.Status,
                DetectionCategory = result.DetectionCategory,
                RawResponseJson = result.RawResponseJson,
                ErrorMessage = result.ErrorMessage,
                FromCache = fromCache,
                CheckedAtUtc = DateTime.UtcNow,
                CacheExpiresAtUtc = result.Status is ThreatIntelligenceStatus.NoThreatDetected or ThreatIntelligenceStatus.ConfirmedMalicious
                    ? DateTime.UtcNow.AddSeconds(provider.CacheTtlSeconds)
                    : null
            });
        }

        await _db.SaveChangesAsync(ct);

        return results.Select(r => new ThreatIntelligenceResultDto(
            r.Provider.Name, r.Provider.Slug, r.Result.Status, r.Result.DetectionCategory, r.Result.ErrorMessage, r.FromCache, DateTime.UtcNow))
            .ToList();
    }

    private async Task<(ThreatIntelligenceProvider Provider, ProviderCheckResult Result, bool FromCache)> CheckOneAsync(
        ThreatIntelligenceProvider provider, string url, string domain, CancellationToken ct)
    {
        var cacheKey = $"ti:{provider.Slug}:{url}";
        var cached = await TryGetCachedAsync(cacheKey, ct);
        if (cached is not null) return (provider, cached, true);

        ProviderCheckResult result;
        try
        {
            result = await _providersBySlug[provider.Slug].CheckAsync(url, domain, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Threat intelligence provider {Provider} failed for {Url}", provider.Slug, url);
            result = ProviderCheckResult.Error(ex.Message);
        }

        if (result.Status is ThreatIntelligenceStatus.NoThreatDetected or ThreatIntelligenceStatus.ConfirmedMalicious)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(provider.CacheTtlSeconds) }, ct);
        }

        return (provider, result, false);
    }

    private async Task<ProviderCheckResult?> TryGetCachedAsync(string cacheKey, CancellationToken ct)
    {
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            return cached is null ? null : JsonSerializer.Deserialize<ProviderCheckResult>(cached);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Cache read failed for {CacheKey}", cacheKey);
            return null;
        }
    }
}
