namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped dashboard owned by a single User (not a Role) — every authenticated
/// user manages their own, the same self-service shape as Employee/Customer/Vendor Portal.
/// Widgets are individually added/removed/reordered, not replaced wholesale, since drag
/// reordering only touches existing rows' DisplayOrder/SizeOption.</summary>
public class Dashboard : BaseEntity
{
    public int    OrganizationId { get; set; }
    public int    UserId         { get; set; }
    public string Name           { get; set; } = string.Empty;
    public bool   IsDefault      { get; set; }

    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}
