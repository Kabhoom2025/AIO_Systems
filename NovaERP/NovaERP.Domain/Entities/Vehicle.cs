namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped delivery vehicle (truck/van) used to physically move Shipments out of
/// a Warehouse. Status is a plain manually/automatically-set string, not a derived state
/// machine — same "keep it simple" reasoning as Shipment.Status before automation was added.</summary>
public class Vehicle : BaseEntity
{
    public int      OrganizationId    { get; set; }
    public string   Code              { get; set; } = string.Empty;
    public string   Name              { get; set; } = string.Empty;
    public string?  Model             { get; set; }
    public decimal  CapacityKg        { get; set; }
    public string   Status            { get; set; } = "Available"; // Available | Loading | InTransit | Maintenance
    public int?     CurrentWarehouseId { get; set; }
    public bool     IsActive          { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Warehouse? CurrentWarehouse { get; set; }
    public ICollection<DeliveryLoad> DeliveryLoads { get; set; } = new List<DeliveryLoad>();
}
