using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.UpdateWorkspaceRole;

public record UpdateWorkspaceRoleCommand(int WorkspaceRoleId, string Name, List<string> Permissions) : IRequest<Result>;
