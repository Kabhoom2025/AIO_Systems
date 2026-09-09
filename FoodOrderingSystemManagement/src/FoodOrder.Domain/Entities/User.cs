namespace FoodOrder.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? OrganizationId { get; set; }

    /// <summary>Null = org-wide (e.g. an Admin who oversees every branch of their organization).</summary>
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;

    public string?   ProfileImage             { get; set; }
    public string?   PasswordResetToken       { get; set; }
    public DateTime? PasswordResetTokenExpiry  { get; set; }

    public Role Role { get; set; } = null!;
    public Organization? Organization { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
