using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Append-only: retries produce new rows with an incrementing AttemptNumber, giving a
/// full inspectable audit trail even mid-execution.</summary>
public class ExecutionStepLog : BaseEntity
{
    public Guid WorkflowExecutionId { get; set; }
    public string NodeKey { get; set; } = string.Empty;
    public WorkflowNodeType NodeType { get; set; }
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public int AttemptNumber { get; set; } = 1;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public string? ErrorMessage { get; set; }

    public WorkflowExecution WorkflowExecution { get; set; } = null!;
}
