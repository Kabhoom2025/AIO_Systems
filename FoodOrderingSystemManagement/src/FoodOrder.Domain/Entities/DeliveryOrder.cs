using FoodOrder.Domain.Enums;

namespace FoodOrder.Domain.Entities;

public class DeliveryOrder : BaseEntity
{
    public int     OrderId           { get; set; }
    public int?    DriverId          { get; set; }
    public DeliveryStatus Status     { get; set; } = DeliveryStatus.Pending;
    public string   DeliveryAddress    { get; set; } = string.Empty;
    public decimal? DeliveryLatitude   { get; set; }
    public decimal? DeliveryLongitude  { get; set; }
    public decimal  DeliveryCharge     { get; set; }
    public string?  CustomerPhone      { get; set; }
    public string? Notes             { get; set; }
    public DateTime? AssignedAt      { get; set; }
    public DateTime? PickedUpAt      { get; set; }
    public DateTime? DeliveredAt     { get; set; }
    public string? ThirdPartyProvider{ get; set; }
    public string? ThirdPartyTrackId { get; set; }

    public Order   Order  { get; set; } = null!;
    public Driver? Driver { get; set; }
}
