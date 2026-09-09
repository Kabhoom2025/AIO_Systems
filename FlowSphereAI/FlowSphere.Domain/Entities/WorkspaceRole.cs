using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>A custom, named permission bag scoped to exactly one workspace (e.g. "Billing Team"
/// on the HRMS workspace) - distinct from the org-global <see cref="Role"/> used for baseline
/// login permissions and account activation.</summary>
public class WorkspaceRole : BaseEntity
{
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Comma-joined subset of PermissionCatalog's workspace-relevant keys.</summary>
    public string Permissions { get; set; } = string.Empty;

    public Workspace Workspace { get; set; } = null!;

    public IReadOnlyCollection<string> PermissionList =>
        Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
