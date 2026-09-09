namespace FoodOrder.Application.DTOs.Order;

public class OrderItemRequestDto
{
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }
    public List<int> AddOnIds { get; set; } = new();
}
