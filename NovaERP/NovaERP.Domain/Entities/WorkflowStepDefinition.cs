namespace NovaERP.Domain.Entities;

/// <summary>One tier of a WorkflowDefinition. MinAmount enables tiered approval — a step only
/// applies to a given WorkflowInstance if Instance.Amount >= MinAmount; a null MinAmount always
/// applies (e.g. a base "manager sign-off on everything" step).</summary>
public class WorkflowStepDefinition : BaseEntity
{
    public int      WorkflowDefinitionId { get; set; }
    public int      StepOrder            { get; set; }
    public string   Name                 { get; set; } = string.Empty;
    public int?     ApproverRoleId       { get; set; }
    public decimal? MinAmount            { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public Role?               ApproverRole       { get; set; }
}
