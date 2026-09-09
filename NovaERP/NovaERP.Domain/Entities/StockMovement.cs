namespace NovaERP.Domain.Entities;

/// <summary>An append-only stock ledger entry for a Product — no Update/Delete, matching how
/// AuditLog is write-once. Quantity is signed: positive increases on-hand, negative decreases
/// it. EntityType/EntityId is the same polymorphic reference pattern as Document/
/// WorkflowInstance, linking back to the PurchaseOrder/SalesOrder that generated the
/// movement (null for manual adjustments).</summary>
public class StockMovement : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      ProductId      { get; set; }
    public int?     WarehouseId    { get; set; }
    public string   MovementType   { get; set; } = "Adjustment"; // Receipt | Issue | Adjustment
    public decimal  Quantity       { get; set; }
    public string?  EntityType     { get; set; }
    public int?     EntityId       { get; set; }
    public DateTime MovementDate   { get; set; } = DateTime.UtcNow;
    public string?  Notes          { get; set; }

    public Organization Organization { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse? Warehouse { get; set; }
}
