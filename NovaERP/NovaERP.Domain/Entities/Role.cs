namespace NovaERP.Domain.Entities;

public class Role : BaseEntity
{
    public int?    OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public bool    IsSystemRole   { get; set; }

    public Organization?              Organization    { get; set; }
    public ICollection<User>          Users           { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission : BaseEntity
{
    public string Key         { get; set; } = string.Empty;
    public string Module      { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : BaseEntity
{
    public int RoleId       { get; set; }
    public int PermissionId { get; set; }

    public Role       Role       { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
