namespace FoodOrder.Application.DTOs.FoodItem;

public class FoodItemDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? Barcode { get; set; }
    public DateTime CreatedDate { get; set; }
}
