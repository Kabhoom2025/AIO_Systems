namespace FoodOrder.Application.DTOs.Category;

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
