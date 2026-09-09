using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.DeleteWorkspaceRole;

public record DeleteWorkspaceRoleCommand(int WorkspaceRoleId) : IRequest<Result>;
