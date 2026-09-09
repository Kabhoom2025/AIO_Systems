namespace Pharmacy.Domain.Entities;

public class Customer : BaseEntity
{
    public int     OrganizationId  { get; set; }
    public string  Name            { get; set; } = string.Empty;
    public string? Phone           { get; set; }
    public string? Email           { get; set; }
    public string? Address         { get; set; }
    public string  MembershipLevel { get; set; } = "Bronze"; // Bronze | Silver | Gold | Platinum
    public int     LoyaltyPoints   { get; set; }
    public decimal WalletBalance   { get; set; }
    public bool    IsActive        { get; set; } = true;

    public Organization Organization { get; set; } = null!;
}
