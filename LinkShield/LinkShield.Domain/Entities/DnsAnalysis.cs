using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class DnsAnalysis : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public bool Resolves { get; set; }
    public string ARecordsJson { get; set; } = "[]";
    public string AaaaRecordsJson { get; set; } = "[]";
    public string MxRecordsJson { get; set; } = "[]";
    public string NsRecordsJson { get; set; } = "[]";
    public string CnameRecordsJson { get; set; } = "[]";
    public string TxtRecordsJson { get; set; } = "[]";

    public bool HasMailConfiguration { get; set; }
    public bool HasSuspiciousNameservers { get; set; }
    public string? LookupError { get; set; }

    public int DnsAnalysisScore { get; set; }
}
