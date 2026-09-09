namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped on/off switch for an entire ERP module (one row per (OrganizationId,
/// ModuleKey), ModuleKey matches a PermissionCatalog module). Lets an org admin see and
/// toggle modules that don't have controllers yet ("coming soon").</summary>
public class FeatureToggle : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string ModuleKey      { get; set; } = string.Empty;
    public bool   IsEnabled      { get; set; }

    public Organization Organization { get; set; } = null!;
}
