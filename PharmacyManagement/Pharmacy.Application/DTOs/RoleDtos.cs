namespace Pharmacy.Application.DTOs;

public class RoleDto
{
    public int          Id             { get; set; }
    public string        Name           { get; set; } = string.Empty;
    public bool          IsSystemRole   { get; set; }
    public List<string>  PermissionKeys { get; set; } = new();
}

public class CreateRoleDto
{
    public string       Name           { get; set; } = string.Empty;
    public List<string> PermissionKeys { get; set; } = new();
}

public class UpdateRoleDto
{
    public string       Name           { get; set; } = string.Empty;
    public List<string> PermissionKeys { get; set; } = new();
}

public class PermissionDto
{
    public int     Id          { get; set; }
    public string  Key         { get; set; } = string.Empty;
    public string  Module      { get; set; } = string.Empty;
    public string? Description { get; set; }
}
