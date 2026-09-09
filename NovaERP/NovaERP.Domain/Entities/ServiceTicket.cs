namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped support ticket. Assign/Resolve/Close/Reopen are explicit actions, not
/// a free-editable Status field — assigning requires naming which Employee picks it up, and
/// resolving requires capturing resolution notes, both real parameters, not just a status flip.
/// Close is terminal; Update/Delete are locked once Closed.</summary>
public class ServiceTicket : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string TicketNumber   { get; set; } = string.Empty;
    public string Subject        { get; set; } = string.Empty;
    public string Description    { get; set; } = string.Empty;
    public int    CategoryId     { get; set; }
    public int    RequesterId    { get; set; }
    public int?   AssignedToId   { get; set; }

    public string  Priority        { get; set; } = "Medium"; // Low | Medium | High | Critical
    public string  Status          { get; set; } = "Open";   // Open | InProgress | Resolved | Closed
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedDate  { get; set; }
    public DateTime? ClosedDate    { get; set; }

    public Organization    Organization { get; set; } = null!;
    public TicketCategory  Category     { get; set; } = null!;
    public Employee        Requester    { get; set; } = null!;
    public Employee?       AssignedTo   { get; set; }
}
