using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Workflows.Queries.GetWorkflowsList;

public record GetWorkflowsListQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<WorkflowSummaryDto>>>;
