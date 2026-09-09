namespace FoodOrder.Domain.Entities;

public class PerformanceReview : BaseEntity
{
    public int      UserId       { get; set; }
    public int      ReviewedById { get; set; }
    public DateTime ReviewDate   { get; set; }
    public int      Rating       { get; set; } // 1–5
    // General | Monthly | Quarterly | Annual
    public string   Category     { get; set; } = "General";
    public string   Comments     { get; set; } = string.Empty;

    public User User       { get; set; } = null!;
    public User ReviewedBy { get; set; } = null!;
}
