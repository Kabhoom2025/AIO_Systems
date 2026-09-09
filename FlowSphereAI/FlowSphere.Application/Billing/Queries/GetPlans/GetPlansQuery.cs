using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Billing.Queries.GetPlans;

public record GetPlansQuery : IRequest<Result<GetPlansResult>>;

public record PlanDto(string Name, int MonthlyExecutionQuota, bool IsCurrent);

public record GetPlansResult(IReadOnlyList<PlanDto> Plans);
