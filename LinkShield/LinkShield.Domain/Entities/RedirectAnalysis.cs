using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class RedirectAnalysis : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public int HopIndex { get; set; }
    public string FromUrl { get; set; } = string.Empty;
    public string ToUrl { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool DomainChanged { get; set; }
    public bool HttpsDowngrade { get; set; }
    public bool TargetIsSuspicious { get; set; }
}
