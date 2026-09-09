namespace NovaERP.Domain.Entities;

/// <summary>An append-only record of moving stock of one Product between two Warehouses.
/// Creating one produces exactly two StockMovement rows (Issue at FromWarehouse, Receipt at
/// ToWarehouse), both referencing this row via EntityType="StockTransfer"/EntityId — same
/// polymorphic-reference-to-generated-Id pattern PO/SO use for their own movements.</summary>
public class StockTransfer : BaseEntity
{
    public int      OrganizationId  { get; set; }
    public int      ProductId       { get; set; }
    public int      FromWarehouseId { get; set; }
    public int      ToWarehouseId   { get; set; }
    public decimal  Quantity        { get; set; }
    public DateTime TransferDate    { get; set; } = DateTime.UtcNow;
    public string?  Notes           { get; set; }

    public Organization Organization { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse FromWarehouse { get; set; } = null!;
    public Warehouse ToWarehouse { get; set; } = null!;
}
