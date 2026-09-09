using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.CreateWorkflow;

public record CreateWorkflowCommand(string Name, string? Description) : IRequest<Result<int>>;
