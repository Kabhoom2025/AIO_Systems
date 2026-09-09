using LinkShield.Domain.Enums;

namespace LinkShield.Application.DTOs.Admin;

public record BrandProfileDto(Guid Id, string BrandName, string OfficialDomain, IReadOnlyList<string> AliasDomains, bool IsEnabled);

public record CreateBrandProfileRequest(string BrandName, string OfficialDomain, IReadOnlyList<string>? AliasDomains);

public record RiskRuleDto(
    Guid Id, string Name, string Description, RiskFactorCategory Category,
    decimal Weight, bool IsEnabled, string ConditionExpression, int ScoreContribution);

public record CreateRiskRuleRequest(
    string Name, string Description, RiskFactorCategory Category,
    decimal Weight, string ConditionExpression, int ScoreContribution);

public record UpdateRiskRuleRequest(
    string Name, string Description, decimal Weight, bool IsEnabled,
    string ConditionExpression, int ScoreContribution);

public record AuditLogDto(
    Guid Id, Guid? UserId, string? UserEmail, AuditAction Action, string EntityType,
    string? EntityId, string? DetailsJson, string? IpAddress, DateTime OccurredAtUtc);
