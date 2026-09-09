namespace NovaERP.Domain.Entities;

/// <summary>Snapshot of one WorkflowStepDefinition for a specific WorkflowInstance.
/// ApproverRoleId is copied from the definition at submit time so history stays stable even if
/// the definition is edited later; ApproverUserId is resolved to the specific acting user only
/// once someone actually approves/rejects.</summary>
public class WorkflowStepInstance : BaseEntity
{
    public int       WorkflowInstanceId { get; set; }
    public int       StepOrder          { get; set; }
    public int?      ApproverRoleId     { get; set; }
    public int?      ApproverUserId     { get; set; }
    public string    Status             { get; set; } = "Pending"; // Pending | Approved | Rejected | Skipped
    public DateTime? ActionedDate       { get; set; }
    public string?   Comments           { get; set; }

    public WorkflowInstance WorkflowInstance { get; set; } = null!;
    public Role?              ApproverRole     { get; set; }
    public User?               ApproverUser    { get; set; }
}
