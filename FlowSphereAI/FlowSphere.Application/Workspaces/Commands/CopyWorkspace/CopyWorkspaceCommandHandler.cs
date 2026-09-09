using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Commands.CopyWorkspace;

/// <summary>Clones a workspace's Apps/Tables into a brand-new workspace - structure only, never
/// runtime data (no AppRecords/TableRecords), and never a promoted app's downstream QA/UAT/Live
/// copies (only the Dev-stage source of truth, since that's the only editable version). Copied
/// apps get a fresh SourceGroupId - a new, independent lineage, not tied to the original's
/// promotion history - and drop LinkedWorkflowDefinitionId/LinkedTableId, since those referenced
/// ids are specific to the source workspace and may not have a copied counterpart.</summary>
public class CopyWorkspaceCommandHandler : IRequestHandler<CopyWorkspaceCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CopyWorkspaceCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CopyWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var source = await _db.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.SourceWorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (source is null)
        {
            return Result<int>.Failure(Error.NotFound($"Workspace {request.SourceWorkspaceId} was not found."));
        }

        var newWorkspace = Workspace.Create(_currentUser.OrganizationId, request.NewWorkspaceName, source.Description, _currentUser.UserId);
        _db.Workspaces.Add(newWorkspace);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.CopyApps)
        {
            var sourceApps = await _db.AppDefinitions
                .Where(a => a.WorkspaceId == request.SourceWorkspaceId && a.Stage == EnvironmentStage.Dev)
                .ToListAsync(cancellationToken);

            foreach (var app in sourceApps)
            {
                _db.AppDefinitions.Add(new AppDefinition
                {
                    WorkspaceId = newWorkspace.Id,
                    Name = app.Name,
                    Description = app.Description,
                    FormSchemaJson = app.FormSchemaJson,
                    BusinessRulesJson = app.BusinessRulesJson,
                    AccessPermissionsJson = app.AccessPermissionsJson,
                    CreatedByUserId = _currentUser.UserId,
                    Stage = EnvironmentStage.Dev,
                    SourceGroupId = Guid.NewGuid(),
                });
            }
        }

        if (request.CopyTables)
        {
            var sourceTables = await _db.TableDefinitions
                .Where(t => t.WorkspaceId == request.SourceWorkspaceId)
                .ToListAsync(cancellationToken);

            foreach (var table in sourceTables)
            {
                _db.TableDefinitions.Add(new TableDefinition
                {
                    WorkspaceId = newWorkspace.Id,
                    Name = table.Name,
                    Description = table.Description,
                    SchemaJson = table.SchemaJson,
                    CreatedByUserId = _currentUser.UserId,
                });
            }
        }

        if (request.AdminUserId is int adminUserId)
        {
            var adminExists = await _db.Users.AnyAsync(u => u.Id == adminUserId, cancellationToken);
            if (adminExists)
            {
                _db.WorkspaceMemberships.Add(new WorkspaceMembership
                {
                    WorkspaceId = newWorkspace.Id,
                    UserId = adminUserId,
                    Role = WorkspaceAdminRole.WorkspaceAdmin,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(newWorkspace.Id);
    }
}
