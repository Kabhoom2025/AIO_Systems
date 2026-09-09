namespace FoodOrder.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int FoodItemId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public string? AddOnNotes { get; set; }
    public bool IsKitchenReady { get; set; } = false;
    public bool IsDelivered   { get; set; } = false;

    public Order Order { get; set; } = null!;
    public FoodItem FoodItem { get; set; } = null!;
}
