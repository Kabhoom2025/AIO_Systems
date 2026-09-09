namespace Pharmacy.Domain.Entities;

public class User : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  Email          { get; set; } = string.Empty;
    public string  PasswordHash   { get; set; } = string.Empty;
    public int     RoleId         { get; set; }
    public int?    BranchId       { get; set; } // null = access to all branches in the org
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Role         Role         { get; set; } = null!;
    public Branch?       Branch       { get; set; }
}
