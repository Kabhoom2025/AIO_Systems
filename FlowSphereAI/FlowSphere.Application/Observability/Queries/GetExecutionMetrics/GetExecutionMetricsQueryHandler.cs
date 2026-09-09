using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Observability.Queries.GetExecutionMetrics;

public class GetExecutionMetricsQueryHandler : IRequestHandler<GetExecutionMetricsQuery, Result<ExecutionMetricsDto>>
{
    private readonly IApplicationDbContext _db;

    public GetExecutionMetricsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ExecutionMetricsDto>> Handle(GetExecutionMetricsQuery request, CancellationToken cancellationToken)
    {
        var windowStart = DateTime.UtcNow.AddHours(-24);

        // WorkflowExecutions is ITenantScoped, so this is already implicitly filtered to the
        // caller's organization by the global query filter.
        var recent = await _db.WorkflowExecutions
            .Where(e => e.CreatedDate >= windowStart)
            .Select(e => new { e.Status, e.StartedAt, e.CompletedAt })
            .ToListAsync(cancellationToken);

        var total = recent.Count;
        var succeeded = recent.Count(e => e.Status == ExecutionStatus.Succeeded);
        var failed = recent.Count(e => e.Status == ExecutionStatus.Failed);
        var runningOrQueuedNow = await _db.WorkflowExecutions
            .CountAsync(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Queued, cancellationToken);

        var completedDurations = recent
            .Where(e => e.StartedAt.HasValue && e.CompletedAt.HasValue)
            .Select(e => (e.CompletedAt!.Value - e.StartedAt!.Value).TotalMilliseconds)
            .ToList();

        var successRate = total == 0 ? 100.0 : Math.Round(100.0 * succeeded / total, 1);
        double? averageDurationMs = completedDurations.Count == 0 ? null : Math.Round(completedDurations.Average(), 1);

        return Result<ExecutionMetricsDto>.Success(new ExecutionMetricsDto(
            total, succeeded, failed, runningOrQueuedNow, successRate, averageDurationMs));
    }
}
