namespace FoodOrder.Domain.Entities;

public class FoodItem : BaseEntity
{
    public int BranchId { get; set; }
    public int CategoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Barcode { get; set; }

    public Branch Branch { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem>      OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ItemAddOn>      ItemAddOns { get; set; } = new List<ItemAddOn>();
    public ICollection<FoodItemRating> Ratings    { get; set; } = new List<FoodItemRating>();
}
