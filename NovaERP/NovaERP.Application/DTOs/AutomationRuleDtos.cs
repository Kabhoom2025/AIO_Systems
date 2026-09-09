namespace NovaERP.Application.DTOs;

public class AutomationRuleDto
{
    public int    Id                    { get; set; }
    public string Name                  { get; set; } = string.Empty;
    public string TriggerEvent          { get; set; } = string.Empty;
    public string ActionType            { get; set; } = string.Empty;
    public int?   NotifyRoleId          { get; set; }
    public string? NotifyRoleName       { get; set; }
    public string NotifyMessageTemplate { get; set; } = string.Empty;
    public bool   IsEnabled             { get; set; }
}

public class CreateAutomationRuleDto
{
    public string Name                  { get; set; } = string.Empty;
    public string TriggerEvent          { get; set; } = string.Empty;
    public string ActionType            { get; set; } = "Notify";
    public int?   NotifyRoleId          { get; set; }
    public string NotifyMessageTemplate { get; set; } = string.Empty;
    public bool   IsEnabled             { get; set; } = true;
}

public class UpdateAutomationRuleDto
{
    public string Name                  { get; set; } = string.Empty;
    public string TriggerEvent          { get; set; } = string.Empty;
    public string ActionType            { get; set; } = "Notify";
    public int?   NotifyRoleId          { get; set; }
    public string NotifyMessageTemplate { get; set; } = string.Empty;
    public bool   IsEnabled             { get; set; } = true;
}
