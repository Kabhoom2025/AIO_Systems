using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class BrandMatch : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public Guid BrandProfileId { get; set; }
    public BrandProfile BrandProfile { get; set; } = null!;

    public string MatchedDomain { get; set; } = string.Empty;
    public string MatchType { get; set; } = string.Empty; // Typosquat, Homoglyph, Misspelling, KeywordCombo
    public double LevenshteinDistance { get; set; }
    public double JaroWinklerSimilarity { get; set; }
    public bool ContainsAuthKeywords { get; set; }
}
