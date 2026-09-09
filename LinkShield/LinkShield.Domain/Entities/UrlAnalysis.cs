using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class UrlAnalysis : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public int UrlLength { get; set; }
    public int DomainLength { get; set; }
    public int PathLength { get; set; }
    public int QueryLength { get; set; }
    public int SubdomainCount { get; set; }
    public int DotCount { get; set; }
    public int HyphenCount { get; set; }
    public int DigitCount { get; set; }
    public int SpecialCharCount { get; set; }

    public bool IsIpAddressHost { get; set; }
    public bool IsHttps { get; set; }
    public bool HasUrlEncoding { get; set; }
    public bool HasDoubleEncoding { get; set; }
    public bool HasPunycode { get; set; }
    public bool HasHomoglyphs { get; set; }
    public bool IsShortenedUrl { get; set; }
    public bool HasSuspiciousTld { get; set; }
    public bool HasRedirectParameter { get; set; }
    public bool HasSuspiciousFileExtension { get; set; }

    public string SuspiciousKeywordsJson { get; set; } = "[]";
    public string SuspiciousParametersJson { get; set; } = "[]";

    public int UrlAnalysisScore { get; set; }
}
