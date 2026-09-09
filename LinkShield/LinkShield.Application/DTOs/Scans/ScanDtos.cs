using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Domain.Enums;

namespace LinkShield.Application.DTOs.Scans;

public record AnalyzeUrlRequestDto(string Url);

public record ScanSummaryDto(
    Guid ScanId,
    string OriginalUrl,
    ScanStatus Status,
    int RiskScore,
    RiskLevel RiskLevel,
    ScanVerdict Verdict,
    DateTime CreatedAtUtc);

public record ScanDetailDto(
    Guid ScanId,
    string OriginalUrl,
    string NormalizedUrl,
    ScanStatus Status,
    string? FailureReason,
    int RiskScore,
    RiskLevel RiskLevel,
    ScanVerdict Verdict,
    decimal Confidence,
    UrlAnalysisResultDto? UrlAnalysis,
    DomainAnalysisResultDto? DomainAnalysis,
    DnsAnalysisResultDto? DnsAnalysis,
    SslAnalysisResultDto? SslAnalysis,
    RedirectAnalysisResultDto? RedirectAnalysis,
    IReadOnlyList<ThreatIntelligenceResultDto> ThreatIntelligenceResults,
    IReadOnlyList<BrandMatchResultDto> BrandMatches,
    MlPredictionResultDto? MlPrediction,
    IReadOnlyList<RiskFactorDto> RiskFactors,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public record PagedResultDto<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
