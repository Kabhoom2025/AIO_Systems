namespace FoodOrder.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Timezone { get; set; }
    public string? Currency { get; set; } = "INR";
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Branch> Branches { get; set; } = [];
}
