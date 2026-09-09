namespace NovaERP.Domain.Entities;

/// <summary>One physical package/carton within a Shipment. All measurement fields are
/// optional — a shipment can be recorded before packing details are finalized.</summary>
public class ShipmentPackage : BaseEntity
{
    public int      ShipmentId     { get; set; }
    public int      PackageNumber  { get; set; }
    public decimal?  WeightKg      { get; set; }
    public decimal?  LengthCm      { get; set; }
    public decimal?  WidthCm       { get; set; }
    public decimal?  HeightCm      { get; set; }
    public string?   TrackingNumber { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public ICollection<ShipmentPackageItem> Items { get; set; } = new List<ShipmentPackageItem>();
}
