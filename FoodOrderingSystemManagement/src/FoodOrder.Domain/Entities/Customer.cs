namespace FoodOrder.Domain.Entities;

public class Customer : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public int     LoyaltyPoints { get; set; } = 0;
    public decimal TotalSpend    { get; set; } = 0;
    public int     TotalVisits   { get; set; } = 0;

    public string Tier => TotalSpend >= 10000 ? "Gold"
                        : TotalSpend >= 3000  ? "Silver"
                        : "Bronze";

    public Organization Organization { get; set; } = null!;
    public ICollection<Order>              Orders              { get; set; } = new List<Order>();
    public ICollection<PointsTransaction>  PointsTransactions  { get; set; } = new List<PointsTransaction>();
    public ICollection<SavedAddress>       SavedAddresses      { get; set; } = new List<SavedAddress>();
}
