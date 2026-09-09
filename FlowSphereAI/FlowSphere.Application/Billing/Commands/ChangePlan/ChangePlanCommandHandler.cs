using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Billing.Commands.ChangePlan;

public class ChangePlanCommandHandler : IRequestHandler<ChangePlanCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ChangePlanCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = PlanCatalog.Find(request.PlanName);
        if (plan is null)
        {
            return Result.Failure(Error.NotFound($"Unknown plan '{request.PlanName}'."));
        }

        var organization = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result.Failure(Error.NotFound("Organization not found."));
        }

        organization.PlanTier = plan.Name;
        organization.MonthlyExecutionQuota = plan.MonthlyExecutionQuota;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
