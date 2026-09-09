namespace FlowSphere.Application.Common;

/// <summary>Shared between GetWorkspaceAdminsQuery and GetWorkspacesListQuery, which both need to
/// project a WorkspaceMembership row down to just the assigned user's id/name.</summary>
public record AdminUserDto(int UserId, string Name);
