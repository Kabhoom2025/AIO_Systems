namespace FoodOrder.Domain.Entities;

public class ItemAddOn
{
    public int FoodItemId { get; set; }
    public int AddOnId { get; set; }

    public FoodItem FoodItem { get; set; } = null!;
    public AddOn AddOn { get; set; } = null!;
}
