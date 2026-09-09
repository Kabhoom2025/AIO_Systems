using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Aggregate root for a single workflow run. Uses a Guid key (not the int
/// BaseEntity.Id convention every other entity uses) so execution ids are safe to expose in
/// URLs/SignalR group messages without leaking sequential row counts. Implements both
/// ITenantScoped and IEnvironmentScoped - the only entity with both dimensions live today - so
/// its global query filter combines OrganizationId and Stage in one predicate.</summary>
public class WorkflowExecution : ITenantScoped, IEnvironmentScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public int OrganizationId { get; set; }
    public EnvironmentStage Stage { get; set; } = EnvironmentStage.Dev;
    public int WorkflowDefinitionId { get; set; }
    public int WorkflowVersionId { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Queued;
    public string TriggerSource { get; set; } = "Manual";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? InputPayloadJson { get; set; }
    public string? ErrorSummary { get; set; }

    /// <summary>Set only while Status == PendingApproval - identifies the User Task node the
    /// execution is suspended at, and the input it should resume with once approved/rejected.
    /// Both are cleared by the engine as soon as it resumes past that node.</summary>
    public string? PendingNodeKey { get; set; }
    public string? PendingResumeInputJson { get; set; }
    public string? PendingResumeHandle { get; set; }

    /// <summary>Interim form input saved via "Enable Save As Draft" on a Step while it's still
    /// pending - never consumed by the execution engine itself, only read back out by the
    /// approval UI to restore what the user typed before they finish resolving the step.</summary>
    public string? PendingDraftDataJson { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowVersion WorkflowVersion { get; set; } = null!;
    public ICollection<ExecutionStepLog> StepLogs { get; set; } = new List<ExecutionStepLog>();

    public static WorkflowExecution CreateQueued(int organizationId, WorkflowVersion version, string? inputPayloadJson, string triggerSource = "Manual", EnvironmentStage stage = EnvironmentStage.Dev)
    {
        return new WorkflowExecution
        {
            OrganizationId = organizationId,
            WorkflowDefinitionId = version.WorkflowDefinitionId,
            WorkflowVersionId = version.Id,
            Status = ExecutionStatus.Queued,
            TriggerSource = triggerSource,
            InputPayloadJson = inputPayloadJson,
            Stage = stage,
        };
    }

    public void MarkRunning()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded()
    {
        Status = ExecutionStatus.Succeeded;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorSummary)
    {
        Status = ExecutionStatus.Failed;
        ErrorSummary = errorSummary;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkPendingApproval(string nodeKey, string? resumeInputJson)
    {
        Status = ExecutionStatus.PendingApproval;
        PendingNodeKey = nodeKey;
        PendingResumeInputJson = resumeInputJson;
    }

    public void ResumeFromPending()
    {
        Status = ExecutionStatus.Running;
        PendingNodeKey = null;
        PendingResumeInputJson = null;
        PendingResumeHandle = null;
        PendingDraftDataJson = null;
    }

    public void SaveDraft(string draftDataJson)
    {
        PendingDraftDataJson = draftDataJson;
    }
}
