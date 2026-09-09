using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Observability.Queries.GetExecutionMetrics;

public record GetExecutionMetricsQuery : IRequest<Result<ExecutionMetricsDto>>;

public record ExecutionMetricsDto(
    int TotalLast24h,
    int SucceededLast24h,
    int FailedLast24h,
    int RunningOrQueuedNow,
    double SuccessRatePercent,
    double? AverageDurationMs);
