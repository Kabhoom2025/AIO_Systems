using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.DeleteAppNotificationRule;

public class DeleteAppNotificationRuleCommandHandler : IRequestHandler<DeleteAppNotificationRuleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DeleteAppNotificationRuleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteAppNotificationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _db.AppNotificationRules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId && r.AppId == request.AppId && r.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (rule is null)
        {
            return Result.Failure(Error.NotFound($"Notification rule {request.RuleId} was not found."));
        }

        _db.AppNotificationRules.Remove(rule);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
