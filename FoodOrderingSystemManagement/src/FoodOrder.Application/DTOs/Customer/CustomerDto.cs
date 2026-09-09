namespace FoodOrder.Application.DTOs.Customer;

public class CustomerDto
{
    public int      Id            { get; set; }
    public string   Name          { get; set; } = string.Empty;
    public string   Phone         { get; set; } = string.Empty;
    public string?  Email         { get; set; }
    public DateTime? Birthday     { get; set; }
    public string?  Notes         { get; set; }
    public bool     IsActive      { get; set; }
    public int      LoyaltyPoints { get; set; }
    public decimal  TotalSpend    { get; set; }
    public int      TotalVisits   { get; set; }
    public string   Tier          { get; set; } = string.Empty;
    public DateTime CreatedDate   { get; set; }
}

public class CreateCustomerRequest
{
    public string   Name     { get; set; } = string.Empty;
    public string   Phone    { get; set; } = string.Empty;
    public string?  Email    { get; set; }
    public DateTime? Birthday { get; set; }
    public string?  Notes    { get; set; }
}

public class UpdateCustomerRequest
{
    public string   Name     { get; set; } = string.Empty;
    public string?  Email    { get; set; }
    public DateTime? Birthday { get; set; }
    public string?  Notes    { get; set; }
    public bool     IsActive { get; set; }
}

public class AdjustPointsRequest
{
    public int    Points      { get; set; }  // positive = add, negative = deduct
    public string Description { get; set; } = string.Empty;
}

public class PointsTransactionDto
{
    public int      Id          { get; set; }
    public int      Points      { get; set; }
    public string   Type        { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    public int?     OrderId     { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class SavedAddressDto
{
    public int     Id          { get; set; }
    public string  Label       { get; set; } = string.Empty;
    public string  AddressLine { get; set; } = string.Empty;
    public string? City        { get; set; }
    public bool    IsDefault   { get; set; }
}

public class AddSavedAddressRequest
{
    public string  Label       { get; set; } = string.Empty;
    public string  AddressLine { get; set; } = string.Empty;
    public string? City        { get; set; }
    public bool    IsDefault   { get; set; }
}

public class CustomerOrderSummaryDto
{
    public int      Id          { get; set; }
    public string   OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate   { get; set; }
    public decimal  GrandTotal  { get; set; }
    public string   Status      { get; set; } = string.Empty;
    public int      PointsEarned   { get; set; }
    public int      PointsRedeemed { get; set; }
}
