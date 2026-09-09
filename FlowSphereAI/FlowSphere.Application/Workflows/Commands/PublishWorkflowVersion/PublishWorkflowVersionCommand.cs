using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.PublishWorkflowVersion;

public record PublishWorkflowVersionCommand(int WorkflowDefinitionId, int VersionId) : IRequest<Result>;
