namespace ProjectFlowAI.Domain.Entities;

/// <summary>
/// A ProjectFlow AI user. OrganizationId is nullable on purpose: null means the account is a
/// platform-level Super Admin that is not scoped to any single tenant. Every later-phase entity
/// (projects, tasks, boards, ...) is expected to carry an OrganizationId FK following this same
/// "null = platform-wide, set = tenant-scoped" convention.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Invited;
    public DateTime? LastLoginAt { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? GoogleId { get; set; }
    public string? MicrosoftId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
}
