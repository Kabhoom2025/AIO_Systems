namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped purchase order — the buy-side counterpart of SalesOrder, with an
/// extra "Received" state marking goods/services actually delivered (no sell-side equivalent
/// is needed there). Optionally links back to the RfqRequest it originated from, mirroring
/// how SalesOrder optionally links to the Opportunity it originated from.</summary>
public class PurchaseOrder : BaseEntity
{
    public int      OrganizationId { get; set; }
    public string   PoNumber       { get; set; } = string.Empty;
    public int      VendorId       { get; set; }
    public int?     RfqRequestId   { get; set; }
    public string   Status         { get; set; } = "Draft"; // Draft | Confirmed | Received | Cancelled
    public DateTime OrderDate      { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public RfqRequest? RfqRequest { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
