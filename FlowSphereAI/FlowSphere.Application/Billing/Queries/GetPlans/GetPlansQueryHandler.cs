using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Billing.Queries.GetPlans;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, Result<GetPlansResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetPlansQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<GetPlansResult>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var organization = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<GetPlansResult>.Failure(Error.NotFound("Organization not found."));
        }

        var plans = PlanCatalog.All
            .Select(p => new PlanDto(p.Name, p.MonthlyExecutionQuota, p.Name == organization.PlanTier))
            .ToList();

        return Result<GetPlansResult>.Success(new GetPlansResult(plans));
    }
}
