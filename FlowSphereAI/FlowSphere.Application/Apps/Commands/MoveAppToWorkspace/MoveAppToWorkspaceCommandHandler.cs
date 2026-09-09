using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.MoveAppToWorkspace;

public class MoveAppToWorkspaceCommandHandler : IRequestHandler<MoveAppToWorkspaceCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public MoveAppToWorkspaceCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(MoveAppToWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be moved while in the Dev sandbox stage."));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        var targetWorkspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.TargetWorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!targetWorkspaceExists)
        {
            return Result.Failure(Error.NotFound($"Workspace {request.TargetWorkspaceId} was not found."));
        }

        if (app.WorkspaceId == request.TargetWorkspaceId)
        {
            return Result.Success();
        }

        var clones = await _db.AppDefinitions
            .Where(a => a.SourceGroupId == app.SourceGroupId && a.Workspace.OrganizationId == _currentUser.OrganizationId)
            .ToListAsync(cancellationToken);

        foreach (var clone in clones)
        {
            if (clone.LinkedTableId is not null)
            {
                var tableExistsInTarget = await _db.TableDefinitions
                    .AnyAsync(t => t.Id == clone.LinkedTableId && t.WorkspaceId == request.TargetWorkspaceId, cancellationToken);

                if (!tableExistsInTarget)
                {
                    clone.LinkTable(null, "{}");
                }
            }

            clone.WorkspaceId = request.TargetWorkspaceId;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
