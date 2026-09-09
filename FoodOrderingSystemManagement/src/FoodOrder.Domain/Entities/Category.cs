namespace FoodOrder.Domain.Entities;

public class Category : BaseEntity
{
    public int BranchId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
