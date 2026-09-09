using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.PromoteApp;

/// <summary>Operates on the target row's own stage directly, not the caller's currently-selected
/// header stage - same rationale as RollbackAppCommandHandler: the Deployments pipeline page
/// manages every stage from one screen, so a Promote click on a QA card must work regardless of
/// whatever stage happens to be selected in the topbar switcher at the time.</summary>
public class PromoteAppCommandHandler : IRequestHandler<PromoteAppCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public PromoteAppCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(PromoteAppCommand request, CancellationToken cancellationToken)
    {
        var source = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (source is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (source.Stage == EnvironmentStage.Live)
        {
            return Result.Failure(Error.Conflict("An app already at the Live stage cannot be promoted further."));
        }

        var targetStage = source.Stage + 1;

        var existingTarget = await _db.AppDefinitions
            .FirstOrDefaultAsync(a => a.SourceGroupId == source.SourceGroupId && a.Stage == targetStage, cancellationToken);

        if (existingTarget is not null)
        {
            existingTarget.ApplyPromotedConfig(source);
        }
        else
        {
            _db.AppDefinitions.Add(source.CloneForPromotion(targetStage));
        }

        _db.DeploymentLogEntries.Add(new DeploymentLogEntry
        {
            OrganizationId = _currentUser.OrganizationId,
            SourceGroupId = source.SourceGroupId,
            AppName = source.Name,
            FromStage = source.Stage,
            ToStage = targetStage,
            Action = DeploymentAction.Promoted,
            PerformedByUserId = _currentUser.UserId,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
