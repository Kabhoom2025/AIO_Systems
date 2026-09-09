namespace FoodOrder.Domain.Entities;

public class Driver : BaseEntity
{
    public string Name        { get; set; } = string.Empty;
    public string Phone       { get; set; } = string.Empty;
    public string? Email      { get; set; }
    public string? VehicleNo  { get; set; }
    public string? VehicleType{ get; set; }
    public bool      IsAvailable         { get; set; } = true;
    public bool      IsActive            { get; set; } = true;
    public int?      OrganizationId      { get; set; }
    public decimal?  CurrentLatitude     { get; set; }
    public decimal?  CurrentLongitude    { get; set; }
    public DateTime? LastLocationUpdate  { get; set; }

    public Organization?           Organization   { get; set; }
    public ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();
}
