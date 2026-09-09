namespace Workflow.Application.Interfaces;

public interface IWorkflowEngine
{
    /// <summary>Runs a specific workflow version's graph against the given context and records a
    /// <see cref="Workflow.Domain.Entities.WorkflowExecution"/>. Used both for "Test Run" (against
    /// the draft) and for real triggers (against the published version).</summary>
    /// <param name="triggerEntityType">"Manual" | "Webhook" — how this run was kicked off.</param>
    /// <param name="contextData">Field values available to condition rules and action templates.</param>
    /// <param name="onStep">Optional callback awaited after each step is recorded — used to stream
    /// progress (e.g. over SSE) while the run executes. The execution row is checkpointed
    /// (persisted incrementally) regardless of whether this is supplied.</param>
    Task<Workflow.Domain.Entities.WorkflowExecution> RunAsync(
        int orgId,
        Workflow.Domain.Entities.WorkflowDefinition definition,
        Workflow.Domain.Entities.WorkflowVersion version,
        string triggerEntityType,
        Dictionary<string, object?> contextData,
        Func<Workflow.Application.Services.WorkflowStepEvent, Task>? onStep = null);
}
