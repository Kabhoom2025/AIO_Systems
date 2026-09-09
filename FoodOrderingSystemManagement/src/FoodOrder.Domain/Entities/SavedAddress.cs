namespace FoodOrder.Domain.Entities;

public class SavedAddress : BaseEntity
{
    public int    CustomerId  { get; set; }
    public string Label       { get; set; } = string.Empty; // Home, Work, Other
    public string AddressLine { get; set; } = string.Empty;
    public string? City       { get; set; }
    public bool   IsDefault   { get; set; } = false;

    public Customer Customer { get; set; } = null!;
}
