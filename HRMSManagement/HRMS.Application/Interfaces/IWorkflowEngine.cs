namespace HRMS.Application.Interfaces;

public interface IWorkflowEngine
{
    /// <summary>
    /// Runs the published workflow (if any) for <paramref name="triggerType"/> in this org against
    /// a business record. No-ops silently if there's no active Published workflow for that trigger —
    /// workflows are purely additive, callers must keep working with none configured.
    /// </summary>
    /// <param name="contextData">Field values available to condition rules, e.g. { "Days": 2m, "DepartmentName": "Engineering" }.</param>
    /// <param name="actionHandlers">Entity-specific actions the caller supports (e.g. "AutoApprove"), keyed by ActionType.
    /// Built-in actions ("Notify", "End") are handled by the engine itself and don't need an entry here.</param>
    Task TriggerAsync(
        int orgId,
        string triggerType,
        string triggerEntityType,
        int triggerEntityId,
        Dictionary<string, object?> contextData,
        Dictionary<string, Func<Dictionary<string, string>, Task>> actionHandlers);
}
