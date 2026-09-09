namespace FoodOrder.Application.DTOs.Order;

public class CreateOrderDto
{
    public int?   TableId         { get; set; }
    public string? CustomerPhone  { get; set; }
    public int?   CustomerId      { get; set; }
    public int    PointsRedeemed  { get; set; } = 0;
    public string OrderType       { get; set; } = "DineIn";   // DineIn | Takeaway | Delivery
    public string? DeliveryAddress{ get; set; }
    public decimal DeliveryCharge { get; set; } = 0;
    public List<OrderItemRequestDto> Items { get; set; } = new();
}
