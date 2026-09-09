namespace FoodOrder.Application.DTOs.Order;

public class AddItemsToOrderDto
{
    public List<OrderItemRequestDto> Items { get; set; } = new();
    public decimal? Discount { get; set; }
}
