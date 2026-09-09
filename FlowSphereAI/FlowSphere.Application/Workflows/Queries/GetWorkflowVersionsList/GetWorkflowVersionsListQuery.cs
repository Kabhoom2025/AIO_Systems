using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowVersionsList;

public record GetWorkflowVersionsListQuery(int WorkflowDefinitionId) : IRequest<Result<List<WorkflowVersionSummaryDto>>>;

public record WorkflowVersionSummaryDto(int Id, int VersionNumber, string Status, DateTime? PublishedAt, DateTime CreatedDate);
