namespace NovaERP.Domain.Entities;

/// <summary>One vehicle-trip: a planned or dispatched consolidation of several Picked Shipments
/// (from the same origin Warehouse) onto one Vehicle, up to its CapacityKg. Dispatching a load
/// doesn't duplicate stock-deduction logic — it delegates each assigned Shipment's Picked→Shipped
/// transition to the existing ShipmentService.ShipAsync, the same "generalize, don't reimplement"
/// reasoning ProductionOrderService/StockTransferService already established.</summary>
public class DeliveryLoad : BaseEntity
{
    public int       OrganizationId { get; set; }
    public string    LoadNumber     { get; set; } = string.Empty;
    public int       VehicleId      { get; set; }
    public int       WarehouseId    { get; set; }
    public DateTime  LoadDate       { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DispatchedDate { get; set; }

    public Organization Organization { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
