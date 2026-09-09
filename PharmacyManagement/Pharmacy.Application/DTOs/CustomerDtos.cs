namespace Pharmacy.Application.DTOs;

public class CustomerDto
{
    public int     Id              { get; set; }
    public string  Name            { get; set; } = string.Empty;
    public string? Phone           { get; set; }
    public string? Email           { get; set; }
    public string? Address         { get; set; }
    public string  MembershipLevel { get; set; } = string.Empty;
    public int     LoyaltyPoints   { get; set; }
    public decimal WalletBalance   { get; set; }
    public bool    IsActive        { get; set; }
}

public class CreateCustomerDto
{
    public string  Name            { get; set; } = string.Empty;
    public string? Phone           { get; set; }
    public string? Email           { get; set; }
    public string? Address         { get; set; }
    public string  MembershipLevel { get; set; } = "Bronze";
}

public class UpdateCustomerDto : CreateCustomerDto
{
    public int     LoyaltyPoints { get; set; }
    public decimal WalletBalance { get; set; }
    public bool    IsActive      { get; set; } = true;
}
