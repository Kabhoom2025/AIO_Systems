namespace FoodOrder.Application.DTOs.Order;

public class OrderItemDto
{
    public int FoodItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? AddOnNotes { get; set; }
    public bool IsKitchenReady { get; set; }
    public bool IsDelivered    { get; set; }
}
