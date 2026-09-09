using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class ThreatIntelligenceResult : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public Guid ThreatIntelligenceProviderId { get; set; }
    public ThreatIntelligenceProvider Provider { get; set; } = null!;

    public string QueriedValue { get; set; } = string.Empty;
    public ThreatIntelligenceStatus Status { get; set; } = ThreatIntelligenceStatus.NoThreatDetected;
    public string? DetectionCategory { get; set; }
    public string RawResponseJson { get; set; } = "{}";
    public string? ErrorMessage { get; set; }
    public bool FromCache { get; set; }
    public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CacheExpiresAtUtc { get; set; }
}
