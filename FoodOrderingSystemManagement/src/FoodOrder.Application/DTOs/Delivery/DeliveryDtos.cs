namespace FoodOrder.Application.DTOs.Delivery;

// ── Driver ───────────────────────────────────────────────────────────────────

public class DriverDto
{
    public int      Id                 { get; set; }
    public string   Name               { get; set; } = string.Empty;
    public string   Phone              { get; set; } = string.Empty;
    public string?  Email              { get; set; }
    public string?  VehicleNo          { get; set; }
    public string?  VehicleType        { get; set; }
    public bool     IsAvailable        { get; set; }
    public bool     IsActive           { get; set; }
    public int      ActiveDeliveries   { get; set; }
    public decimal? CurrentLatitude    { get; set; }
    public decimal? CurrentLongitude   { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
}

public class UpdateDriverLocationDto
{
    public decimal Latitude  { get; set; }
    public decimal Longitude { get; set; }
}

public class CreateDriverDto
{
    public string  Name        { get; set; } = string.Empty;
    public string  Phone       { get; set; } = string.Empty;
    public string? Email       { get; set; }
    public string? VehicleNo   { get; set; }
    public string? VehicleType { get; set; }
}

public class UpdateDriverDto
{
    public string  Name        { get; set; } = string.Empty;
    public string  Phone       { get; set; } = string.Empty;
    public string? Email       { get; set; }
    public string? VehicleNo   { get; set; }
    public string? VehicleType { get; set; }
    public bool    IsAvailable { get; set; }
    public bool    IsActive    { get; set; }
}

// ── Delivery Order ───────────────────────────────────────────────────────────

public class DeliveryOrderDto
{
    public int      Id                { get; set; }
    public int      OrderId           { get; set; }
    public string   OrderNumber       { get; set; } = string.Empty;
    public decimal  OrderTotal        { get; set; }
    public int?     DriverId          { get; set; }
    public string?  DriverName        { get; set; }
    public string?  DriverPhone       { get; set; }
    public string   Status            { get; set; } = string.Empty;
    public string   DeliveryAddress    { get; set; } = string.Empty;
    public decimal? DeliveryLatitude   { get; set; }
    public decimal? DeliveryLongitude  { get; set; }
    public decimal  DeliveryCharge     { get; set; }
    public string?  CustomerPhone      { get; set; }
    public string?  Notes             { get; set; }
    public DateTime? AssignedAt       { get; set; }
    public DateTime? PickedUpAt       { get; set; }
    public DateTime? DeliveredAt      { get; set; }
    public string?  ThirdPartyProvider{ get; set; }
    public string?  ThirdPartyTrackId { get; set; }
    public DateTime CreatedDate       { get; set; }
}

public class CreateDeliveryOrderDto
{
    public int      OrderId           { get; set; }
    public string   DeliveryAddress   { get; set; } = string.Empty;
    public decimal? DeliveryLatitude  { get; set; }
    public decimal? DeliveryLongitude { get; set; }
    public decimal  DeliveryCharge    { get; set; }
    public string?  CustomerPhone     { get; set; }
    public string?  Notes             { get; set; }
}

public class AssignDriverDto
{
    public int DriverId { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public string  Status { get; set; } = string.Empty;
    public string? Notes  { get; set; }
}

// ── Delivery Charge Slab ─────────────────────────────────────────────────────

public class DeliveryChargeSlabDto
{
    public int     Id      { get; set; }
    public decimal FromKm  { get; set; }
    public decimal ToKm    { get; set; }
    public decimal Charge  { get; set; }
}

public class UpsertDeliveryChargeSlabDto
{
    public decimal FromKm { get; set; }
    public decimal ToKm   { get; set; }
    public decimal Charge { get; set; }
}

// ── Third Party Config ───────────────────────────────────────────────────────

public class ThirdPartyConfigDto
{
    public int     Id         { get; set; }
    public string  Provider   { get; set; } = string.Empty;
    public string  ApiKey     { get; set; } = string.Empty;
    public string? ApiSecret  { get; set; }
    public string? WebhookUrl { get; set; }
    public bool    IsEnabled  { get; set; }
}

public class UpsertThirdPartyConfigDto
{
    public string  Provider   { get; set; } = string.Empty;
    public string  ApiKey     { get; set; } = string.Empty;
    public string? ApiSecret  { get; set; }
    public string? WebhookUrl { get; set; }
    public bool    IsEnabled  { get; set; }
}

// ── Third Party Dispatch ─────────────────────────────────────────────────────

public class DispatchToProviderDto
{
    public string Provider { get; set; } = string.Empty;
}

// ── Dashboard Stats ──────────────────────────────────────────────────────────

public class DeliveryDashboardDto
{
    public int     TotalToday      { get; set; }
    public int     PendingCount    { get; set; }
    public int     ActiveCount     { get; set; }
    public int     DeliveredToday  { get; set; }
    public int     FailedToday     { get; set; }
    public int     AvailableDrivers{ get; set; }
    public decimal TotalChargeToday{ get; set; }
}
