using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Tags a user as a Workspace Admin or Data Admin of a specific workspace - visibility
/// and assignment only. Does NOT yet gate any app/table mutation (those remain governed solely by
/// the existing global PermissionCatalog policies) - a deliberate scope line, not an oversight,
/// since enforcing this would mean touching every App/Table command handler.</summary>
public class WorkspaceMembership : BaseEntity
{
    public int WorkspaceId { get; set; }
    public int UserId { get; set; }
    public WorkspaceAdminRole Role { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
}
