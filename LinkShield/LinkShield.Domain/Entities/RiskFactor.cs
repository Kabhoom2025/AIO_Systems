using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class RiskFactor : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public Guid? RiskRuleId { get; set; }
    public RiskRule? RiskRule { get; set; }

    public RiskFactorCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ScoreContribution { get; set; }
}
