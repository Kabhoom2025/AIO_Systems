using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.CreateWorkspace;

public record CreateWorkspaceCommand(string Name, string? Description) : IRequest<Result<int>>;
