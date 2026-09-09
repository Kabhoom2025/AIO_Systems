using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Executions.Queries.GetPendingApprovals;

/// <summary>Cross-workflow list of executions currently suspended at a UserTask (accept/reject)
/// step - e.g. leave requests waiting on HR - unlike GetExecutionHistoryQuery, which is scoped to
/// a single workflow.</summary>
public record GetPendingApprovalsQuery(int Page = 1, int PageSize = 20, int? WorkflowDefinitionId = null) : IRequest<Result<PagedResult<PendingApprovalDto>>>;

public record PendingApprovalDto(Guid Id, int WorkflowDefinitionId, string WorkflowName, string TriggerSource, DateTime? StartedAt, string? AssigneeLabel);
