using Hangfire;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Jobs;

/// <summary>Registers/removes a Hangfire recurring job per Scheduled-trigger WorkflowDefinition.
/// Mirrors NovaERP.Infrastructure.Jobs.RecurringJobRegistrar's role, adapted to this app's
/// per-row (not per-JobKey) scheduling model — every Scheduled workflow gets its own recurring job
/// keyed by its own id, since (unlike NovaERP's fixed job-class catalog) the action list is
/// data-driven per WorkflowDefinition rather than a fixed set of C# job classes.</summary>
public class HangfireWorkflowScheduler : IWorkflowScheduler
{
    public static string RecurringJobId(Guid workflowDefinitionId) => $"workflow-{workflowDefinitionId}";

    public void RegisterOrUpdate(Guid workflowDefinitionId, string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression)) return;
        RecurringJob.AddOrUpdate<ScheduledWorkflowJob>(
            RecurringJobId(workflowDefinitionId),
            job => job.RunAsync(workflowDefinitionId),
            cronExpression);
    }

    public void Remove(Guid workflowDefinitionId) => RecurringJob.RemoveIfExists(RecurringJobId(workflowDefinitionId));
}
