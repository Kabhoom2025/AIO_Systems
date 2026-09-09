namespace NovaERP.Domain.Entities;

/// <summary>A live approval run for one business entity (EntityType/EntityId, the same
/// polymorphic pattern Document.EntityType/EntityId already uses in this codebase). Amount, when
/// present, selects which WorkflowStepDefinition tiers apply for this specific run.</summary>
public class WorkflowInstance : BaseEntity
{
    public int      WorkflowDefinitionId { get; set; }
    public string   EntityType           { get; set; } = string.Empty;
    public int      EntityId             { get; set; }
    public decimal? Amount               { get; set; }
    public string   Status               { get; set; } = "Pending"; // Pending | Approved | Rejected | Cancelled
    public int      CurrentStepOrder     { get; set; }
    public int      SubmittedByUserId    { get; set; }
    public DateTime SubmittedDate        { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate       { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public User                SubmittedByUser    { get; set; } = null!;
    public ICollection<WorkflowStepInstance> Steps { get; set; } = new List<WorkflowStepInstance>();
}
