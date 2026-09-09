using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class DomainAnalysis : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public string Domain { get; set; } = string.Empty;
    public string RegisteredDomain { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Tld { get; set; } = string.Empty;

    public string? Registrar { get; set; }
    public DateTime? RegisteredAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int? DomainAgeDays { get; set; }
    public string? Organization { get; set; }
    public string? Country { get; set; }
    public string NameserversJson { get; set; } = "[]";

    public bool RdapLookupSucceeded { get; set; }
    public string? RdapError { get; set; }

    public int DomainAnalysisScore { get; set; }
}
