namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped sales order. Stops at "Confirmed" — invoicing belongs to the
/// Finance module (Phase 5, not built yet). Lines are owned/replaced wholesale, same as
/// TaxCode's Components and WorkflowDefinition's Steps.</summary>
public class SalesOrder : BaseEntity
{
    public int       OrganizationId { get; set; }
    public string    OrderNumber    { get; set; } = string.Empty;
    public int       AccountId      { get; set; }
    public int?       OpportunityId { get; set; }
    public string    Status         { get; set; } = "Draft"; // Draft | Confirmed | Cancelled
    public DateTime  OrderDate      { get; set; } = DateTime.UtcNow.Date;
    public int       OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Opportunity? Opportunity { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
