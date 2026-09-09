namespace Pharmacy.Application.DTOs;

public class UserSummaryDto
{
    public int     Id       { get; set; }
    public string  Name     { get; set; } = string.Empty;
    public string  Email    { get; set; } = string.Empty;
    public string  RoleName { get; set; } = string.Empty;
    public int?    BranchId { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}
