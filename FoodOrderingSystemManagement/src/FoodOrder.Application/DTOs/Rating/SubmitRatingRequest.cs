namespace FoodOrder.Application.DTOs.Rating;

public class SubmitRatingRequest
{
    public int    FoodItemId   { get; set; }
    public int    Rating       { get; set; }   // 1–5
    public string? Comment     { get; set; }
    public string  SessionId   { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
}
