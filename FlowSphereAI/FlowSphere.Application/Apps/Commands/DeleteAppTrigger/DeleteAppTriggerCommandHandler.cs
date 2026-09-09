using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.DeleteAppTrigger;

public class DeleteAppTriggerCommandHandler : IRequestHandler<DeleteAppTriggerCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DeleteAppTriggerCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteAppTriggerCommand request, CancellationToken cancellationToken)
    {
        var trigger = await _db.AppTriggers
            .FirstOrDefaultAsync(t => t.Id == request.TriggerId && t.SourceAppId == request.SourceAppId && t.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (trigger is null)
        {
            return Result.Failure(Error.NotFound($"Trigger {request.TriggerId} was not found."));
        }

        _db.AppTriggers.Remove(trigger);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
