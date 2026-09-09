using LinkShield.Domain.Enums;

namespace LinkShield.Application.DTOs.ThreatIntelligence;

public record ProviderCheckResult(ThreatIntelligenceStatus Status, string? DetectionCategory, string RawResponseJson, string? ErrorMessage)
{
    public static ProviderCheckResult NoThreat(string rawJson) => new(ThreatIntelligenceStatus.NoThreatDetected, null, rawJson, null);
    public static ProviderCheckResult Malicious(string category, string rawJson) => new(ThreatIntelligenceStatus.ConfirmedMalicious, category, rawJson, null);
    public static ProviderCheckResult Unavailable(string reason) => new(ThreatIntelligenceStatus.Unavailable, null, "{}", reason);
    public static ProviderCheckResult Error(string message) => new(ThreatIntelligenceStatus.Error, null, "{}", message);
}

public record ThreatIntelligenceResultDto(
    string ProviderName,
    string ProviderSlug,
    ThreatIntelligenceStatus Status,
    string? DetectionCategory,
    string? ErrorMessage,
    bool FromCache,
    DateTime CheckedAtUtc);
