namespace Pharmacy.Domain.Entities;

public class Role : BaseEntity
{
    public int?    OrganizationId { get; set; } // null = system-wide default role
    public string  Name           { get; set; } = string.Empty;
    public bool    IsSystemRole   { get; set; }

    public Organization?               Organization    { get; set; }
    public ICollection<User>           Users           { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
