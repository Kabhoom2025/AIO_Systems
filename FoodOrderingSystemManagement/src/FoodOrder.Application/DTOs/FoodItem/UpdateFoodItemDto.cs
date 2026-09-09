namespace FoodOrder.Application.DTOs.FoodItem;

public class UpdateFoodItemDto
{
    public int CategoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? Barcode { get; set; }
}
