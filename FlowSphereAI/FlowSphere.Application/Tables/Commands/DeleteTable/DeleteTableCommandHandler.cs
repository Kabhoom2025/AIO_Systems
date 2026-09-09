using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Commands.DeleteTable;

public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DeleteTableCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var table = await _db.TableDefinitions
            .Include(t => t.Workspace)
            .FirstOrDefaultAsync(t => t.Id == request.TableId && t.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (table is null)
        {
            return Result.Failure(Error.NotFound($"Table {request.TableId} was not found."));
        }

        // LinkedTableId is a plain nullable column, not an FK constraint, so apps that integrate
        // with this table need explicit unlinking or they'd be left pointing at a deleted row.
        var linkedApps = await _db.AppDefinitions
            .Where(a => a.LinkedTableId == request.TableId)
            .ToListAsync(cancellationToken);
        foreach (var app in linkedApps)
        {
            app.LinkTable(null, "{}");
        }

        // TableRecords cascade-delete via their FK (see TableRecordConfiguration).
        _db.TableDefinitions.Remove(table);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
