namespace NovaERP.Domain.Entities;

/// <summary>One line on a PurchaseOrder. ItemName is always free text (kept as a fallback for
/// non-catalog items); ProductId optionally links to the Phase 4 Product catalog — when set,
/// receiving the order records a Receipt stock movement for it. TaxRatePercent is snapshotted
/// from the referenced TaxCode's components sum at save time, identical to
/// SalesOrderLine.TaxRatePercent.</summary>
public class PurchaseOrderLine : BaseEntity
{
    public int     PurchaseOrderId { get; set; }
    public string  ItemName        { get; set; } = string.Empty;
    public int?    ProductId       { get; set; }
    public decimal Quantity        { get; set; }
    public decimal UnitPrice       { get; set; }
    public int?    TaxCodeId       { get; set; }
    public decimal TaxRatePercent  { get; set; }
    public int     DisplayOrder    { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product? Product { get; set; }
    public TaxCode? TaxCode { get; set; }
}
