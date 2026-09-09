using LinkShield.Application.DTOs.Risk;

namespace LinkShield.Application.Interfaces;

/// <summary>Combines every stage's signals into one explainable 0-100 score (spec section 12).
/// Category weights are read from SystemSettings (admin-configurable) rather than hardcoded.
/// Never claims "Malicious" from scoring alone — only a confirmed threat-intelligence hit
/// produces that verdict (spec section 33); scoring-only signals top out at Suspicious.</summary>
public interface IRiskEngine
{
    Task<RiskEngineResultDto> ComputeAsync(RiskEngineInput input, CancellationToken ct = default);
}

public interface IMlPredictionClient
{
    /// <summary>Returns null if the ml-service is unreachable or has no trained model yet —
    /// never fabricates a prediction (spec section 33/14).</summary>
    Task<MlPredictionResultDto?> PredictAsync(Guid scanId, MlFeaturesDto features, CancellationToken ct = default);
}

public record MlFeaturesDto(
    int UrlLength, int DomainLength, int PathLength, int QueryLength,
    int SubdomainCount, int DotCount, int HyphenCount, int DigitCount, int SpecialCharCount,
    bool IsIpAddressHost, bool IsHttps, bool HasUrlEncoding, bool HasPunycode,
    bool IsShortenedUrl, bool HasSuspiciousTld, bool HasRedirectParameter,
    int? DomainAgeDays, int RedirectCount, bool HasMailConfiguration,
    bool SslIsValid, bool SslDomainMatches);
