using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

public class Role : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Comma-joined permission keys, e.g. "workflows.read,workflows.write" - mirrors
    /// the claims-based RBAC convention already used by sibling services in this repo.</summary>
    public string Permissions { get; set; } = string.Empty;

    public Organization Organization { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();

    public IReadOnlyCollection<string> PermissionList =>
        Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
