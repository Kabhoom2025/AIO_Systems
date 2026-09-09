using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.ExecuteWorkflow;

public record ExecuteWorkflowCommand(int WorkflowDefinitionId, string? InputPayloadJson) : IRequest<Result<Guid>>;
