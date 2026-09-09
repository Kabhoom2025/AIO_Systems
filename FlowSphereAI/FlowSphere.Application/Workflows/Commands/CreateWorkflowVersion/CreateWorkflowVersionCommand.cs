using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.CreateWorkflowVersion;

public record CreateWorkflowVersionCommand(int WorkflowDefinitionId, string GraphJson) : IRequest<Result<CreatedVersionDto>>;

public record CreatedVersionDto(int VersionId, int VersionNumber);
