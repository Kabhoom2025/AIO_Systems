namespace AIO_Systems.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? OrganizationId { get; set; }

    /// <summary>
    /// Null = org-wide. AIO_Systems has no Branch entity/UI of its own yet — this only
    /// exists so the JWT it mints can carry a branchId claim through to FoodOrder.API,
    /// which does own real Branch data. Always null until a Branch management UI exists here.
    /// </summary>
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;

    public string?   ProfileImage             { get; set; }
    public string?   PasswordResetToken       { get; set; }
    public DateTime? PasswordResetTokenExpiry  { get; set; }

    public Role Role { get; set; } = null!;
    public Organization? Organization { get; set; }
}
