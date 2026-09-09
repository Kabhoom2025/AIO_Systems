namespace FoodOrder.Application.DTOs.Rating;

public class RecommendedItemDto
{
    public int     FoodItemId         { get; set; }
    public string  ItemName           { get; set; } = string.Empty;
    public string  CategoryName       { get; set; } = string.Empty;
    public decimal Price              { get; set; }
    public string? Image              { get; set; }
    public double  AverageRating      { get; set; }
    public int     RatingCount        { get; set; }
    public int     OrderCount         { get; set; }
    public double  Score              { get; set; }
    public string  RecommendationTag  { get; set; } = string.Empty;
}
