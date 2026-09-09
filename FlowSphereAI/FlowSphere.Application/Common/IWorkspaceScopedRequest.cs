namespace FlowSphere.Application.Common;

/// <summary>Opts a command into WorkspacePermissionBehavior's real per-workspace permission
/// check: the caller must have RequiredPermission either globally (their org Role) or via a
/// WorkspaceRoleAssignment scoped to WorkspaceId, or the request is rejected before the handler
/// runs.</summary>
public interface IWorkspaceScopedRequest
{
    int WorkspaceId { get; }
    string RequiredPermission { get; }
}

/// <summary>Same enforcement as IWorkspaceScopedRequest, but for commands that only carry an
/// AppId (most Apps write commands) - WorkspacePermissionBehavior resolves the owning
/// WorkspaceId via a lookup instead of requiring every command to carry it directly.</summary>
public interface IAppScopedRequest
{
    int AppId { get; }
    string RequiredPermission { get; }
}

/// <summary>Same as IAppScopedRequest, resolved via TableDefinition instead.</summary>
public interface ITableScopedRequest
{
    int TableId { get; }
    string RequiredPermission { get; }
}
