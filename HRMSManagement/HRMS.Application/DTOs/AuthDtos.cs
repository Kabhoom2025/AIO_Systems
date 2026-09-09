namespace HRMS.Application.DTOs;

public class LoginDto
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token            { get; set; } = string.Empty;
    public string RefreshToken     { get; set; } = string.Empty;
    public int    UserId           { get; set; }
    public int?   EmployeeId       { get; set; }
    public string Name             { get; set; } = string.Empty;
    public string Email            { get; set; } = string.Empty;
    public string Role             { get; set; } = string.Empty;
    public int    OrganizationId   { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token       { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class LoginHistoryDto
{
    public int      Id            { get; set; }
    public string   Email         { get; set; } = string.Empty;
    public bool     Success       { get; set; }
    public string?  IpAddress     { get; set; }
    public string?  UserAgent     { get; set; }
    public string?  FailureReason { get; set; }
    public DateTime CreatedDate   { get; set; }
}
