using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class SslAnalysis : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public bool HasCertificate { get; set; }
    public bool IsValid { get; set; }
    public string? Issuer { get; set; }
    public string? Subject { get; set; }
    public string SubjectAlternativeNamesJson { get; set; } = "[]";
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public bool IsExpired { get; set; }
    public bool IsSelfSigned { get; set; }
    public bool DomainMatchesCertificate { get; set; }
    public string? TlsVersion { get; set; }
    public string? ChainValidationError { get; set; }

    public int SslAnalysisScore { get; set; }
}
