namespace Pharmacy.Domain.Entities;

public class Permission : BaseEntity
{
    public string  Key         { get; set; } = string.Empty; // e.g. "medicines.view"
    public string  Module      { get; set; } = string.Empty; // e.g. "Medicines"
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
