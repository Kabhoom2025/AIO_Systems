using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>Grants a user a <see cref="WorkspaceRole"/> within a specific workspace. Actively
/// enforced by WorkspacePermissionBehavior for workspace-scoped write/submit commands - not just
/// a visibility record (contrast with <see cref="WorkspaceMembership"/>).</summary>
public class WorkspaceRoleAssignment : BaseEntity
{
    public int WorkspaceId { get; set; }
    public int UserId { get; set; }
    public int WorkspaceRoleId { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
    public WorkspaceRole WorkspaceRole { get; set; } = null!;
}
