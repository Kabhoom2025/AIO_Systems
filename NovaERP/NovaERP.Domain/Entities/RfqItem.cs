namespace NovaERP.Domain.Entities;

/// <summary>One requested item on an RfqRequest. No Product catalog exists yet (Phase 4),
/// so ItemName is free text — same reasoning as SalesOrderLine.ItemName.</summary>
public class RfqItem : BaseEntity
{
    public int     RfqRequestId { get; set; }
    public string  ItemName     { get; set; } = string.Empty;
    public decimal Quantity     { get; set; }
    public int     DisplayOrder { get; set; }

    public RfqRequest RfqRequest { get; set; } = null!;
}
