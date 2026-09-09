using LinkShield.Application.Interfaces;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.ApiKeyAuth;

/// <summary>
/// Request-time validation for X-API-Key: hash the incoming key, look it up, check
/// active/expiry/client-active, enforce the client's per-minute rate limit and daily quota.
/// The daily quota is enforced by counting UrlScans created by this client since midnight UTC —
/// a DB read-then-compare rather than an atomic counter, so it's not airtight under heavy
/// concurrent bursts from the same key. Good enough for the current traffic level; revisit with
/// a proper atomic counter (Redis INCR + EXPIRE) if this ever becomes a real bottleneck.
/// </summary>
public class ApiKeyValidator : IApiKeyValidator
{
    private readonly LinkShieldDbContext _db;
    private readonly IApiKeyRateLimiter _rateLimiter;

    public ApiKeyValidator(LinkShieldDbContext db, IApiKeyRateLimiter rateLimiter)
    {
        _db = db;
        _rateLimiter = rateLimiter;
    }

    public async Task<ApiKeyValidationResult> ValidateAndRecordUsageAsync(string rawKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return new ApiKeyValidationResult(false, null, null, "Missing API key.");

        var keyHash = ApiKeyHasher.Hash(rawKey);
        var apiKey = await _db.ApiKeys
            .Include(k => k.ApiClient)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, ct);

        if (apiKey is null) return new ApiKeyValidationResult(false, null, null, "Invalid API key.");
        if (!apiKey.IsActive) return new ApiKeyValidationResult(false, null, null, "API key is revoked or expired.");
        if (!apiKey.ApiClient.IsActive) return new ApiKeyValidationResult(false, null, null, "API client account is inactive.");

        if (!_rateLimiter.TryConsume(apiKey.ApiClientId, apiKey.ApiClient.RequestsPerMinute))
            return new ApiKeyValidationResult(false, apiKey.ApiClientId, apiKey.ApiClient.OrganizationName, "Per-minute rate limit exceeded.");

        var todayStart = DateTime.UtcNow.Date;
        var usedToday = await _db.UrlScans.CountAsync(s => s.ApiClientId == apiKey.ApiClientId && s.CreatedAtUtc >= todayStart, ct);
        if (usedToday >= apiKey.ApiClient.DailyQuota)
            return new ApiKeyValidationResult(false, apiKey.ApiClientId, apiKey.ApiClient.OrganizationName, "Daily quota exceeded.");

        apiKey.LastUsedAtUtc = DateTime.UtcNow;
        apiKey.UsageCount++;
        await _db.SaveChangesAsync(ct);

        return new ApiKeyValidationResult(true, apiKey.ApiClientId, apiKey.ApiClient.OrganizationName, null);
    }
}
