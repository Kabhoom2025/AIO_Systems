namespace NovaERP.Application.Interfaces;

/// <summary>Raises a business event to the automation subsystem. Callers (e.g.
/// IWorkflowInstanceService) pass a small string/string context bag — placeholders like
/// {EntityType}/{EntityId}/{Amount} in an AutomationRule.NotifyMessageTemplate are substituted
/// from these keys via plain string.Replace, no templating engine.</summary>
public interface IAutomationEngine
{
    Task HandleEventAsync(int organizationId, string triggerEvent, IDictionary<string, string> context);
}
