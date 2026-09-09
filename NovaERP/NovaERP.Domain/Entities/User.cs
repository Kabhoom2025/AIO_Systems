namespace NovaERP.Domain.Entities;

public class User : BaseEntity
{
    public int       OrganizationId  { get; set; }
    public int?       BranchId       { get; set; }
    public string    Name            { get; set; } = string.Empty;
    public string    Email           { get; set; } = string.Empty;
    public string    PasswordHash    { get; set; } = string.Empty;
    public int       RoleId          { get; set; }
    public bool      IsActive        { get; set; } = true;
    public DateTime? LastLoginAt     { get; set; }
    public int       FailedLoginCount { get; set; }
    public DateTime? LockedUntil     { get; set; }
    public string?   PhotoData        { get; set; }
    public string?   PhotoContentType { get; set; }

    public Organization Organization { get; set; } = null!;
    public Role         Role         { get; set; } = null!;
    public Branch?       Branch      { get; set; }
}

public class RefreshToken : BaseEntity
{
    public int       UserId            { get; set; }
    public string    Token             { get; set; } = string.Empty;
    public DateTime  ExpiresAt         { get; set; }
    public DateTime? RevokedAt         { get; set; }
    public string?   ReplacedByToken   { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}

public class PasswordResetToken : BaseEntity
{
    public int      UserId    { get; set; }
    public string   Token     { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool     IsUsed    { get; set; }

    public User User { get; set; } = null!;
}

public class LoginHistory : BaseEntity
{
    public int?    UserId        { get; set; }
    public string  Email         { get; set; } = string.Empty;
    public bool    Success       { get; set; }
    public string? IpAddress     { get; set; }
    public string? UserAgent     { get; set; }
    public string? FailureReason { get; set; }

    public User? User { get; set; }
}
