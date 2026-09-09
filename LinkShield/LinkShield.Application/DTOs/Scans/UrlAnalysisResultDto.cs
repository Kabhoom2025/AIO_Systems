namespace LinkShield.Application.DTOs.Scans;

/// <summary>
/// Result of the URL-analysis stage (spec section 5) — purely structural signals computed
/// from the URL string itself, no network calls. Mirrors the <c>UrlAnalysis</c> entity plus
/// a provisional stage-local score: this is NOT the final risk score (that's the risk engine,
/// spec section 12, combining every stage) — it only reflects what this one stage found.
/// </summary>
public record UrlAnalysisResultDto(
    string OriginalUrl,
    string NormalizedUrl,
    int UrlLength,
    int DomainLength,
    int PathLength,
    int QueryLength,
    int SubdomainCount,
    int DotCount,
    int HyphenCount,
    int DigitCount,
    int SpecialCharCount,
    bool IsIpAddressHost,
    bool IsHttps,
    bool HasUrlEncoding,
    bool HasDoubleEncoding,
    bool HasPunycode,
    bool HasHomoglyphs,
    bool IsShortenedUrl,
    bool HasSuspiciousTld,
    bool HasRedirectParameter,
    bool HasSuspiciousFileExtension,
    IReadOnlyList<string> SuspiciousKeywords,
    IReadOnlyList<string> SuspiciousParameters,
    int UrlAnalysisScore);
