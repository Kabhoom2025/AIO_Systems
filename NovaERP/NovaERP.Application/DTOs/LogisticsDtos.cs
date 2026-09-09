namespace NovaERP.Application.DTOs;

public class VehicleDto
{
    public int      Id                  { get; set; }
    public string   Code                { get; set; } = string.Empty;
    public string   Name                { get; set; } = string.Empty;
    public string?  Model               { get; set; }
    public decimal  CapacityKg          { get; set; }
    public string   Status              { get; set; } = string.Empty;
    public int?     CurrentWarehouseId  { get; set; }
    public string?  CurrentWarehouseName { get; set; }
    public bool     IsActive            { get; set; }
}

public class CreateVehicleDto
{
    public string   Code               { get; set; } = string.Empty;
    public string   Name               { get; set; } = string.Empty;
    public string?  Model              { get; set; }
    public decimal  CapacityKg         { get; set; }
    public int?     CurrentWarehouseId { get; set; }
    public bool     IsActive           { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Product.Sku/Warehouse.Code.
/// Status isn't editable here either — it's only ever changed by the dispatch/mark-available
/// actions, so a manual edit can't silently contradict an in-progress DeliveryLoad.</summary>
public class UpdateVehicleDto
{
    public string   Name               { get; set; } = string.Empty;
    public string?  Model              { get; set; }
    public decimal  CapacityKg         { get; set; }
    public int?     CurrentWarehouseId { get; set; }
    public bool     IsActive           { get; set; } = true;
}

public class DeliveryLoadShipmentDto
{
    public int      ShipmentId       { get; set; }
    public string   ShipmentNumber   { get; set; } = string.Empty;
    public string?  ReferenceLabel   { get; set; } // e.g. "SO-00002" or "Transfer to Pune"
    public string?  ShipToCity       { get; set; }
    public decimal  TotalWeightKg    { get; set; }
    public int      LineCount        { get; set; }
}

public class DeliveryLoadDto
{
    public int      Id             { get; set; }
    public string   LoadNumber     { get; set; } = string.Empty;
    public int      VehicleId      { get; set; }
    public string   VehicleCode    { get; set; } = string.Empty;
    public string   VehicleName    { get; set; } = string.Empty;
    public decimal  VehicleCapacityKg { get; set; }
    public int      WarehouseId    { get; set; }
    public string   WarehouseName  { get; set; } = string.Empty;
    public DateTime LoadDate       { get; set; }
    public DateTime? DispatchedDate { get; set; }
    public decimal  TotalWeightKg  { get; set; }
    public List<DeliveryLoadShipmentDto> Shipments { get; set; } = new();
}

public class CreateDeliveryLoadDto
{
    public int VehicleId   { get; set; }
    public int WarehouseId { get; set; }
}

public class AssignShipmentDto
{
    public int ShipmentId { get; set; }
}
