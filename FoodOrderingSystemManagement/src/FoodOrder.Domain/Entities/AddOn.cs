namespace FoodOrder.Domain.Entities;

public class AddOn : BaseEntity
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; } // e.g., 'topping', 'cheese', 'sauce'
    public bool IsAvailable { get; set; } = true;

    public ICollection<ItemAddOn> ItemAddOns { get; set; } = new List<ItemAddOn>();
}
