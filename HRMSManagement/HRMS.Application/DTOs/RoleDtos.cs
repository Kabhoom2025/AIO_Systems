namespace HRMS.Application.DTOs;

public class RoleDto
{
    public int          Id             { get; set; }
    public string       Name           { get; set; } = string.Empty;
    public string?      Description    { get; set; }
    public bool         IsSystemRole   { get; set; }
    public List<string> PermissionKeys { get; set; } = new();
    public int          UserCount      { get; set; }
}

public class CreateRoleDto
{
    public string       Name           { get; set; } = string.Empty;
    public string?      Description    { get; set; }
    public List<string> PermissionKeys { get; set; } = new();
}

public class UpdateRoleDto
{
    public string       Name           { get; set; } = string.Empty;
    public string?      Description    { get; set; }
    public List<string> PermissionKeys { get; set; } = new();
}
