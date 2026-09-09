using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Dashboard.Queries.GetDashboardKpis;

public record GetDashboardKpisQuery : IRequest<Result<DashboardKpisDto>>;

public record DashboardKpisDto(
    string PlanTier,
    int MonthlyExecutionQuota,
    int ExecutionsThisMonth,
    int TotalWorkflows,
    int SucceededCount,
    int FailedCount);
