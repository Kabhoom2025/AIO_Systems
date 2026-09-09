using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Tests.TestDoubles;

/// <summary>Test double for handlers that depend on IWorkflowEngine but whose tests aren't
/// exercising workflow automation — mirrors NoOpNotificationDispatcher.</summary>
public class NoOpWorkflowEngine : IWorkflowEngine
{
    public Task EvaluateTriggerAsync(WorkflowTriggerType triggerType, Guid projectId, WorkItem? triggerEntity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task ExecuteScheduledWorkflowAsync(Guid workflowDefinitionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task ResumeRunAsync(Guid workflowRunId, bool approved, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
