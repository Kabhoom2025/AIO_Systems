using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.CreateApp;

public class CreateAppCommandHandler : IRequestHandler<CreateAppCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public CreateAppCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<int>> Handle(CreateAppCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result<int>.Failure(Error.Conflict("Apps can only be created while in the Dev sandbox stage."));
        }

        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<int>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var app = AppDefinition.Create(request.WorkspaceId, request.Name, request.Description, _currentUser.UserId, request.Icon);

        _db.AppDefinitions.Add(app);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(app.Id);
    }
}
