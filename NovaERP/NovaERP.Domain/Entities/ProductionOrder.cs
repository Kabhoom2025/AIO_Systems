namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped order to manufacture a quantity of a finished-good Product at a
/// single Warehouse. Completing it consumes the Product's BillOfMaterial components and
/// produces the finished good, all as StockMovement rows tagged EntityType="ProductionOrder".
/// Single-warehouse only — no cross-warehouse component sourcing in this pass.</summary>
public class ProductionOrder : BaseEntity
{
    public int      OrganizationId { get; set; }
    public string   MoNumber       { get; set; } = string.Empty;
    public int      ProductId      { get; set; }
    public int      WarehouseId    { get; set; }
    public decimal  Quantity       { get; set; }
    public string   Status         { get; set; } = "Draft"; // Draft | Completed | Cancelled
    public DateTime OrderDate      { get; set; } = DateTime.UtcNow.Date;
    public int      OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User Owner { get; set; } = null!;
}
