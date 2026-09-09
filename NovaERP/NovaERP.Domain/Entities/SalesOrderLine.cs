namespace NovaERP.Domain.Entities;

/// <summary>One line on a SalesOrder. ItemName is always free text (kept as a fallback for
/// non-catalog items); ProductId optionally links to the Phase 4 Product catalog — when set,
/// confirming the order records an Issue stock movement for it. TaxRatePercent is snapshotted
/// from the referenced TaxCode's components sum at the moment the line is saved, so historical
/// order totals don't shift if tax rates change later — same snapshot reasoning as
/// WorkflowStepInstance.ApproverRoleId.</summary>
public class SalesOrderLine : BaseEntity
{
    public int     SalesOrderId   { get; set; }
    public string  ItemName       { get; set; } = string.Empty;
    public int?    ProductId      { get; set; }
    public decimal Quantity       { get; set; }
    public decimal UnitPrice      { get; set; }
    public int?    TaxCodeId      { get; set; }
    public decimal TaxRatePercent { get; set; }
    public int     DisplayOrder   { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
    public Product? Product { get; set; }
    public TaxCode? TaxCode { get; set; }
}
