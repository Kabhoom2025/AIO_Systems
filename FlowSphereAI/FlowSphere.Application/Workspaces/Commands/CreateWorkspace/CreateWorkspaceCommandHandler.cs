using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;

namespace FlowSphere.Application.Workspaces.Commands.CreateWorkspace;

public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CreateWorkspaceCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = Workspace.Create(
            _currentUser.OrganizationId, request.Name, request.Description, _currentUser.UserId);

        _db.Workspaces.Add(workspace);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(workspace.Id);
    }
}
