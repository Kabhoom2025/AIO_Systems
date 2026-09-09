using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.RemoveWorkspaceRoleAssignment;

public record RemoveWorkspaceRoleAssignmentCommand(int WorkspaceId, int UserId, int WorkspaceRoleId) : IRequest<Result>;
