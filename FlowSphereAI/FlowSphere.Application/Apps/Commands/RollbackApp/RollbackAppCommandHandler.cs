using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.RollbackApp;

/// <summary>Deletes an app's copy at its current stage - e.g. QA rejects it, so the QA row (and
/// its QA test data, which cascade-deletes via AppRecord's FK) is discarded. The Dev copy is a
/// separate row, untouched by this, so it's already there ready for fixes and a fresh re-promote.
/// Operates on the row's own stage directly, not the caller's currently-selected header stage -
/// the Deployments pipeline page manages every stage from one screen regardless of which stage
/// happens to be selected in the topbar switcher.</summary>
public class RollbackAppCommandHandler : IRequestHandler<RollbackAppCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public RollbackAppCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RollbackAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (app.Stage == EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("The Dev copy is the original - there's nothing to roll it back to."));
        }

        var fromStage = app.Stage;
        var toStage = fromStage - 1;

        _db.DeploymentLogEntries.Add(new DeploymentLogEntry
        {
            OrganizationId = _currentUser.OrganizationId,
            SourceGroupId = app.SourceGroupId,
            AppName = app.Name,
            FromStage = fromStage,
            ToStage = toStage,
            Action = DeploymentAction.RolledBack,
            PerformedByUserId = _currentUser.UserId,
        });

        _db.AppDefinitions.Remove(app);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
