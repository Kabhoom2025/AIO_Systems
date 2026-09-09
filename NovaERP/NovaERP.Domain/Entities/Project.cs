namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped project. Status is edited directly via the standard Update
/// endpoint — no dedicated start/complete/cancel action methods and no locked-state
/// Update/Delete guard, unlike every financial/order-document module: a project's status is
/// a fluid management-plane classification (can move back and forth between Active/OnHold),
/// not a one-way audit trail.</summary>
public class Project : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public int     ManagerId      { get; set; }

    public DateTime  StartDate { get; set; }
    public DateTime? EndDate   { get; set; }
    public decimal?  Budget    { get; set; }
    public string    Status    { get; set; } = "Planning"; // Planning | Active | OnHold | Completed | Cancelled

    public Organization Organization { get; set; } = null!;
    public Employee     Manager      { get; set; } = null!;
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
