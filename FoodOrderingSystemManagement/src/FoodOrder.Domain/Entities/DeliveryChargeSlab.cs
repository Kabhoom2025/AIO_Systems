namespace FoodOrder.Domain.Entities;

public class DeliveryChargeSlab : BaseEntity
{
    public decimal FromKm   { get; set; }
    public decimal ToKm     { get; set; }
    public decimal Charge   { get; set; }
    public int?    OrganizationId { get; set; }

    public Organization? Organization { get; set; }
}
