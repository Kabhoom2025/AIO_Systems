using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Commands.CreateTable;

public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CreateTableCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<int>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var table = TableDefinition.Create(request.WorkspaceId, request.Name, request.Description, _currentUser.UserId);

        _db.TableDefinitions.Add(table);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(table.Id);
    }
}
