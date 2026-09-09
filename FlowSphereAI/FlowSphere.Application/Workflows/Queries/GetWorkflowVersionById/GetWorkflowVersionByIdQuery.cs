using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowVersionById;

public record GetWorkflowVersionByIdQuery(int WorkflowDefinitionId, int VersionId) : IRequest<Result<WorkflowVersionDetailDto>>;

public record WorkflowVersionDetailDto(int Id, int VersionNumber, string Status, string GraphJson, DateTime? PublishedAt);
