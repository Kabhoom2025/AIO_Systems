using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Commands.PublishTable;

public class PublishTableCommandHandler : IRequestHandler<PublishTableCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public PublishTableCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(PublishTableCommand request, CancellationToken cancellationToken)
    {
        var table = await _db.TableDefinitions
            .Include(t => t.Workspace)
            .FirstOrDefaultAsync(t => t.Id == request.TableId && t.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (table is null)
        {
            return Result.Failure(Error.NotFound($"Table {request.TableId} was not found."));
        }

        if (!HasAtLeastOneColumn(table.SchemaJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["schema"] = new[] { "Add at least one column to the table before publishing." },
            }));
        }

        table.Publish();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool HasAtLeastOneColumn(string schemaJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(schemaJson);
            return doc.RootElement.TryGetProperty("columns", out var columnsEl)
                && columnsEl.ValueKind == JsonValueKind.Array
                && columnsEl.GetArrayLength() > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
