using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class BrandProfile : BaseEntity
{
    public string BrandName { get; set; } = string.Empty;
    public string OfficialDomain { get; set; } = string.Empty;
    public string AliasDomainsJson { get; set; } = "[]";
    public bool IsEnabled { get; set; } = true;

    public ICollection<BrandMatch> Matches { get; set; } = new List<BrandMatch>();
}
