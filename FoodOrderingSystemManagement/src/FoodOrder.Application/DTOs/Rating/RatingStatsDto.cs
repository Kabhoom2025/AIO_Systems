namespace FoodOrder.Application.DTOs.Rating;

public class RatingStatsDto
{
    public int     FoodItemId    { get; set; }
    public double  AverageRating { get; set; }
    public int     RatingCount   { get; set; }
}
