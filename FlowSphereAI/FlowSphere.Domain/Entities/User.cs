using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

public class User : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Null until an admin assigns a global role - the user sits in the Pending state
    /// (see <see cref="IsEmailVerified"/>) until both this and email verification are set.</summary>
    public int? RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public int? ManagerId { get; set; }

    public Organization Organization { get; set; } = null!;
    public Role? Role { get; set; }
    public User? Manager { get; set; }
}
