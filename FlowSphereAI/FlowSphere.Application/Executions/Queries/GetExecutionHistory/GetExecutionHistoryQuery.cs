using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Executions.Queries.GetExecutionHistory;

public record GetExecutionHistoryQuery(int WorkflowDefinitionId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ExecutionSummaryDto>>>;

public record ExecutionSummaryDto(Guid Id, string Status, string TriggerSource, DateTime? StartedAt, DateTime? CompletedAt);
