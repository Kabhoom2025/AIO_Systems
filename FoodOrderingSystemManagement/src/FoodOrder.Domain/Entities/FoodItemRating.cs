namespace FoodOrder.Domain.Entities;

public class FoodItemRating : BaseEntity
{
    public int    FoodItemId   { get; set; }
    public int    Rating       { get; set; }          // 1–5
    public string? Comment     { get; set; }
    public string  SessionId   { get; set; } = string.Empty;  // browser session — one rating per session per item
    public string? TableNumber { get; set; }

    public FoodItem FoodItem { get; set; } = null!;
}
