namespace NovaERP.Domain.Entities;

/// <summary>Generic, entity-agnostic approval workflow template. EntityType is free text
/// (e.g. "ExpenseClaim", "PurchaseOrder") — future business modules plug into the engine by
/// submitting a WorkflowInstance for their own entity type, no FK to a concrete module table.</summary>
public class WorkflowDefinition : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string EntityType     { get; set; } = string.Empty;
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<WorkflowStepDefinition> Steps { get; set; } = new List<WorkflowStepDefinition>();
}
