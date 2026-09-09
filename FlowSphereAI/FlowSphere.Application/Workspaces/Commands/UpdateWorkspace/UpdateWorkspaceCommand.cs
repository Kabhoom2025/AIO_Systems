using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.UpdateWorkspace;

public record UpdateWorkspaceCommand(int WorkspaceId, string Name, string? Description) : IRequest<Result>;
