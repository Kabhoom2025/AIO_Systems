using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Workflows.Commands.PromoteWorkflowVersion;

public record PromoteWorkflowVersionCommand(int WorkflowDefinitionId, int VersionId, EnvironmentStage TargetStage) : IRequest<Result>;
