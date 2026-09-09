using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class RiskRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskFactorCategory Category { get; set; }
    public decimal Weight { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string ConditionExpression { get; set; } = string.Empty;
    public int ScoreContribution { get; set; }
}
