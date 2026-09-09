using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.LinkAppWorkflow;

public class LinkAppWorkflowCommandHandler : IRequestHandler<LinkAppWorkflowCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public LinkAppWorkflowCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(LinkAppWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (request.WorkflowDefinitionId is not null)
        {
            // WorkflowDefinitions is ITenantScoped, so this is already implicitly filtered to the
            // current organization by the global query filter.
            var workflowExists = await _db.WorkflowDefinitions
                .AnyAsync(w => w.Id == request.WorkflowDefinitionId, cancellationToken);

            if (!workflowExists)
            {
                return Result.Failure(Error.NotFound($"Workflow {request.WorkflowDefinitionId} was not found."));
            }
        }

        app.LinkWorkflow(request.WorkflowDefinitionId);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
