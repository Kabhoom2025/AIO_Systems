namespace FoodOrder.Domain.Entities;

public class License : BaseEntity
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Plan { get; set; } = "Basic";   // Basic | Pro | Enterprise
    public string Status { get; set; } = "Trial"; // Active | Trial | Expired
    public DateTime ExpiryDate { get; set; }
    public int MaxUsers { get; set; } = 5;
    public string? Notes { get; set; }
}
