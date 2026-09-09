namespace NovaERP.Domain.Entities;

/// <summary>Named ProjectTask, not Task — Task would collide with System.Threading.Tasks.Task
/// in virtually every async service file in this codebase, the same "avoid a colliding name"
/// reasoning that already named LedgerAccount instead of Account. A full independent CRUD
/// entity, not an owned/replaced-wholesale line collection on Project — tasks are a living,
/// ever-changing work-item list, not a fixed transaction snapshot.</summary>
public class ProjectTask : BaseEntity
{
    public int  OrganizationId { get; set; }
    public int  ProjectId      { get; set; }
    public int? AssignedToId   { get; set; }

    public string  Title       { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  Priority    { get; set; } = "Medium"; // Low | Medium | High
    public string  Status      { get; set; } = "ToDo";   // ToDo | InProgress | Done | Blocked

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate   { get; set; }

    public Organization Organization { get; set; } = null!;
    public Project       Project     { get; set; } = null!;
    public Employee?     AssignedTo  { get; set; }
}
