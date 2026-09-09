namespace NovaERP.Domain.Entities;

/// <summary>Lightweight "on event X, do Y" automation hook — deliberately not a JSON rule/DSL
/// engine, consistent with this codebase's plain-typed-column style. TriggerEvent values come
/// from NovaERP.Application.Common.AutomationEvents. ActionType is currently only "Notify";
/// other values are reserved extension points (see IAutomationEngine).</summary>
public class AutomationRule : BaseEntity
{
    public int     OrganizationId        { get; set; }
    public string  Name                  { get; set; } = string.Empty;
    public string  TriggerEvent          { get; set; } = string.Empty;
    public string  ActionType            { get; set; } = "Notify";
    public int?    NotifyRoleId          { get; set; }
    public string  NotifyMessageTemplate { get; set; } = string.Empty;
    public bool    IsEnabled             { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Role?         NotifyRole   { get; set; }
}
