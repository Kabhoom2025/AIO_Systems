namespace LinkShield.Application.DTOs.Analysis;

public record DomainAnalysisResultDto(
    string Domain,
    string RegisteredDomain,
    string Subdomain,
    string Tld,
    string? Registrar,
    DateTime? RegisteredAtUtc,
    DateTime? ExpiresAtUtc,
    int? DomainAgeDays,
    string? Organization,
    string? Country,
    IReadOnlyList<string> Nameservers,
    bool RdapLookupSucceeded,
    string? RdapError,
    int DomainAnalysisScore);

public record DnsAnalysisResultDto(
    bool Resolves,
    IReadOnlyList<string> ARecords,
    IReadOnlyList<string> AaaaRecords,
    IReadOnlyList<string> MxRecords,
    IReadOnlyList<string> NsRecords,
    IReadOnlyList<string> CnameRecords,
    IReadOnlyList<string> TxtRecords,
    bool HasMailConfiguration,
    bool HasSuspiciousNameservers,
    string? LookupError,
    int DnsAnalysisScore);

public record SslAnalysisResultDto(
    bool HasCertificate,
    bool IsValid,
    string? Issuer,
    string? Subject,
    IReadOnlyList<string> SubjectAlternativeNames,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    bool IsExpired,
    bool IsSelfSigned,
    bool DomainMatchesCertificate,
    string? TlsVersion,
    string? ChainValidationError,
    int SslAnalysisScore);

public record RedirectHopDto(
    int HopIndex,
    string FromUrl,
    string ToUrl,
    int StatusCode,
    bool DomainChanged,
    bool HttpsDowngrade,
    bool TargetIsSuspicious);

public record RedirectAnalysisResultDto(
    string FinalUrl,
    int RedirectCount,
    bool AnyDomainChange,
    bool AnyHttpsDowngrade,
    IReadOnlyList<RedirectHopDto> Hops,
    string? Error,
    int RedirectAnalysisScore);
