namespace NovaERP.Application.DTOs;

public class UserDto
{
    public int       Id          { get; set; }
    public string    Name        { get; set; } = string.Empty;
    public string    Email       { get; set; } = string.Empty;
    public int       RoleId      { get; set; }
    public string    RoleName    { get; set; } = string.Empty;
    public int?      BranchId    { get; set; }
    public string?   BranchName  { get; set; }
    public bool      IsActive    { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime  CreatedDate { get; set; }
    /// <summary>Ready-to-bind data URL ("data:image/png;base64,...") — null when no photo is
    /// set. Computed on read from PhotoData/PhotoContentType so the client never needs a
    /// separate authenticated download round-trip just to render an &lt;img&gt;.</summary>
    public string?   PhotoUrl    { get; set; }
}

public class CreateUserDto
{
    public string Name     { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int    RoleId   { get; set; }
    public int?   BranchId { get; set; }
    /// <summary>Raw base64 (no "data:" prefix) of the selected photo — optional.</summary>
    public string? PhotoData        { get; set; }
    public string? PhotoContentType { get; set; }
}

public class UpdateUserDto
{
    public string Name     { get; set; } = string.Empty;
    public int    RoleId   { get; set; }
    public int?   BranchId { get; set; }
    public bool   IsActive { get; set; }
    /// <summary>Null means "leave the existing photo unchanged"; an empty string means
    /// "remove the photo" — same tri-state convention used for optional fields elsewhere.</summary>
    public string? PhotoData        { get; set; }
    public string? PhotoContentType { get; set; }
    public bool    RemovePhoto      { get; set; }
}

public class ResetUserPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
