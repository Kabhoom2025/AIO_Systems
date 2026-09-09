using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Executions.Queries.GetPendingApprovals;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, Result<PagedResult<PendingApprovalDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPendingApprovalsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<PendingApprovalDto>>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.WorkflowExecutions.Where(e => e.Status == ExecutionStatus.PendingApproval);

        if (request.WorkflowDefinitionId is int wfId)
        {
            query = query.Where(e => e.WorkflowDefinitionId == wfId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(e => e.CreatedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new
            {
                e.Id,
                e.WorkflowDefinitionId,
                WorkflowName = e.WorkflowDefinition.Name,
                e.TriggerSource,
                e.StartedAt,
                e.PendingNodeKey,
                GraphJson = e.WorkflowVersion.GraphJson,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(e => new PendingApprovalDto(
                e.Id, e.WorkflowDefinitionId, e.WorkflowName, e.TriggerSource, e.StartedAt,
                WorkflowGraphNodeConfig.GetAssigneeLabel(e.GraphJson, e.PendingNodeKey)))
            .ToList();

        return Result<PagedResult<PendingApprovalDto>>.Success(new PagedResult<PendingApprovalDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
