using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.CreateWorkspaceRole;

public record CreateWorkspaceRoleCommand(int WorkspaceId, string Name, List<string> Permissions) : IRequest<Result<int>>;
