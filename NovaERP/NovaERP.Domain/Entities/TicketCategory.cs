namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped ticket-category reference data (e.g. "Hardware", "Software"). Plain CRUD,
/// same shape as AssetCategory/LeaveType — no lifecycle.</summary>
public class TicketCategory : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string Code           { get; set; } = string.Empty;
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<ServiceTicket> Tickets { get; set; } = new List<ServiceTicket>();
}
