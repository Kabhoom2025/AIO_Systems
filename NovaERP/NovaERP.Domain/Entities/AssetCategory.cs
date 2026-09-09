namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped asset-category reference data (e.g. "Laptop", "Vehicle"). Plain CRUD,
/// same shape as LeaveType — no lifecycle.</summary>
public class AssetCategory : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string Code           { get; set; } = string.Empty;
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
