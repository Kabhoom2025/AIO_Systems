using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.AssignWorkspaceRole;

public record AssignWorkspaceRoleCommand(int WorkspaceId, int UserId, int WorkspaceRoleId) : IRequest<Result>;
