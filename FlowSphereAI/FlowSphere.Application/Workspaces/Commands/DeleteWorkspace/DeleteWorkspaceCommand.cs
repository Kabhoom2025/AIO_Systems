using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand(int WorkspaceId) : IRequest<Result>;
