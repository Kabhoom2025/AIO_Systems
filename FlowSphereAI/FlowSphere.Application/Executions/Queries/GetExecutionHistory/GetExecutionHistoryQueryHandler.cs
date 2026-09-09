using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Executions.Queries.GetExecutionHistory;

public class GetExecutionHistoryQueryHandler : IRequestHandler<GetExecutionHistoryQuery, Result<PagedResult<ExecutionSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetExecutionHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<ExecutionSummaryDto>>> Handle(GetExecutionHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _db.WorkflowExecutions.Where(e => e.WorkflowDefinitionId == request.WorkflowDefinitionId);

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(e => e.CreatedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new { e.Id, e.Status, e.TriggerSource, e.StartedAt, e.CompletedAt })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(e => new ExecutionSummaryDto(e.Id, e.Status.ToString(), e.TriggerSource, e.StartedAt, e.CompletedAt))
            .ToList();

        return Result<PagedResult<ExecutionSummaryDto>>.Success(new PagedResult<ExecutionSummaryDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
