using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class ThreatIntelligenceProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKeySecretName { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 5000;
    public int CacheTtlSeconds { get; set; } = 3600;
    public decimal WeightMultiplier { get; set; } = 1.0m;

    public ICollection<ThreatIntelligenceResult> Results { get; set; } = new List<ThreatIntelligenceResult>();
}
