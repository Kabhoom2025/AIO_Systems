using LinkShield.Application.DTOs.ThreatIntelligence;

namespace LinkShield.Application.Interfaces;

/// <summary>One adapter per threat-intelligence provider. Adding a new provider means adding
/// a new class that implements this — the risk engine and aggregator never change. A provider
/// with no API key configured reports Unavailable rather than skipping silently or faking a
/// result, so gaps in coverage are visible in the stored ThreatIntelligenceResult rows.</summary>
public interface IThreatIntelligenceProvider
{
    /// <summary>Must match a seeded ThreatIntelligenceProvider.Slug row exactly.</summary>
    string ProviderSlug { get; }

    Task<ProviderCheckResult> CheckAsync(string url, string domain, CancellationToken ct = default);
}

public interface IThreatIntelligenceService
{
    Task<IReadOnlyList<ThreatIntelligenceResultDto>> CheckAllAsync(Guid urlScanId, string url, string domain, CancellationToken ct = default);
}
