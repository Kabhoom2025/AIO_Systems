using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetWorkspaceRoles;

public record GetWorkspaceRolesQuery(int? WorkspaceId) : IRequest<Result<IReadOnlyList<WorkspaceRoleDto>>>;

public record WorkspaceRoleDto(int Id, int WorkspaceId, string WorkspaceName, string Name, List<string> Permissions);
