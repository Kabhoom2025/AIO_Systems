using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.DiscardApp;

/// <summary>Deletes a Dev-stage app outright - the honest version of Quixy's "discard changes,"
/// since this codebase doesn't track field-level edit history to revert partial changes, only
/// "this row exists or it doesn't." Only allowed while the app has never been promoted anywhere -
/// once a QA/UAT/Live copy exists, use Rollback on that copy instead (Discard is for undoing work
/// still confined to Dev, not for reverting something already shared downstream).</summary>
public class DiscardAppCommandHandler : IRequestHandler<DiscardAppCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DiscardAppCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DiscardAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (app.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Only a Dev-stage app can be discarded - roll back its promoted copy instead."));
        }

        var hasBeenPromoted = await _db.AppDefinitions
            .AnyAsync(a => a.SourceGroupId == app.SourceGroupId && a.Stage != EnvironmentStage.Dev, cancellationToken);

        if (hasBeenPromoted)
        {
            return Result.Failure(Error.Conflict("This app has already been promoted - discard only applies before the first promotion. Use Rollback instead."));
        }

        _db.AppDefinitions.Remove(app);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
