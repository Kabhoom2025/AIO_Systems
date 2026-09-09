using ProjectFlowAI.Application.Features.Automation;

namespace ProjectFlowAI.Infrastructure.Jobs;

/// <summary>Hangfire executes this class's RunAsync method on the configured cron cadence for one
/// Scheduled-trigger WorkflowDefinition. Scheduled workflows have no single triggering WorkItem —
/// there is nothing to evaluate conditions against — so they just execute their configured actions
/// unconditionally (e.g. "send a notification to the PM every Monday").</summary>
public class ScheduledWorkflowJob
{
    private readonly IWorkflowEngine _engine;

    public ScheduledWorkflowJob(IWorkflowEngine engine) => _engine = engine;

    public Task RunAsync(Guid workflowDefinitionId) => _engine.ExecuteScheduledWorkflowAsync(workflowDefinitionId);
}
