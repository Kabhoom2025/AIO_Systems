using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.AssignWorkspaceAdmin;

public record AssignWorkspaceAdminCommand(int WorkspaceId, int UserId, WorkspaceAdminRole Role) : IRequest<Result>;
