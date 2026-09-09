namespace FlowSphere.Domain.Enums;

public enum ExecutionStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled,
    PendingApproval
}
