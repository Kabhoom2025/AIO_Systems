namespace FoodOrder.Domain.Entities;

public class PointsTransaction : BaseEntity
{
    public int    CustomerId  { get; set; }
    public int    Points      { get; set; }  // positive = earn, negative = redeem/deduct
    public string Type        { get; set; } = "Earn"; // Earn | Redeem | Adjust
    public string Description { get; set; } = string.Empty;
    public int?   OrderId     { get; set; }

    public Customer Customer { get; set; } = null!;
    public Order?   Order    { get; set; }
}
