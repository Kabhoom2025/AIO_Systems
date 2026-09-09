using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Domain.Enums;

namespace LinkShield.Application.DTOs.Risk;

public record MlPredictionResultDto(string Prediction, decimal Probability, string ModelVersion);

public record RiskFactorDto(RiskFactorCategory Category, string Description, int ScoreContribution);

public record RiskEngineInput(
    UrlAnalysisResultDto UrlAnalysis,
    DomainAnalysisResultDto? DomainAnalysis,
    DnsAnalysisResultDto? DnsAnalysis,
    SslAnalysisResultDto? SslAnalysis,
    RedirectAnalysisResultDto? RedirectAnalysis,
    IReadOnlyList<ThreatIntelligenceResultDto> ThreatIntelligenceResults,
    IReadOnlyList<BrandMatchResultDto> BrandMatches,
    MlPredictionResultDto? MlPrediction);

public record RiskEngineResultDto(
    int RiskScore,
    RiskLevel RiskLevel,
    ScanVerdict Verdict,
    decimal Confidence,
    IReadOnlyList<RiskFactorDto> Factors);
