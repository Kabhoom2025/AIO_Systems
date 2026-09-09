using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.RemoveWorkspaceAdmin;

public record RemoveWorkspaceAdminCommand(int WorkspaceId, int UserId, WorkspaceAdminRole Role) : IRequest<Result>;
