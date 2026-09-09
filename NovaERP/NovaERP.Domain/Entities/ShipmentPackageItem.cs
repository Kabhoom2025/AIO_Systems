namespace NovaERP.Domain.Entities;

/// <summary>How much of a given product is packed into one physical ShipmentPackage — lets a
/// single ShipmentLine's quantity be split across multiple packages, and lets the package's
/// weight be derived from what's actually packed in it (Product.WeightKg * Quantity summed).</summary>
public class ShipmentPackageItem : BaseEntity
{
    public int     ShipmentPackageId { get; set; }
    public int     ProductId         { get; set; }
    public decimal Quantity          { get; set; }

    public ShipmentPackage Package { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
