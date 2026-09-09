namespace HRMS.Application.DTOs;

public class UserDto
{
    public int       Id           { get; set; }
    public string    Name         { get; set; } = string.Empty;
    public string    Email        { get; set; } = string.Empty;
    public int       RoleId       { get; set; }
    public string    RoleName     { get; set; } = string.Empty;
    public int?      BranchId     { get; set; }
    public string?   BranchName   { get; set; }
    public int?      EmployeeId   { get; set; }
    public string?   EmployeeName { get; set; }
    public bool      IsActive     { get; set; }
    public DateTime? LastLoginAt  { get; set; }
    public DateTime  CreatedDate  { get; set; }
}

public class CreateUserDto
{
    public string Name       { get; set; } = string.Empty;
    public string Email      { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
    public int    RoleId     { get; set; }
    public int?   BranchId   { get; set; }
    public int?   EmployeeId { get; set; }
}

public class UpdateUserDto
{
    public string Name       { get; set; } = string.Empty;
    public int    RoleId     { get; set; }
    public int?   BranchId   { get; set; }
    public int?   EmployeeId { get; set; }
    public bool   IsActive   { get; set; }
}

public class ResetUserPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
