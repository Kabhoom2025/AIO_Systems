using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Commands.SaveTableSchema;

public class SaveTableSchemaCommandHandler : IRequestHandler<SaveTableSchemaCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public SaveTableSchemaCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SaveTableSchemaCommand request, CancellationToken cancellationToken)
    {
        if (!IsValidTableSchema(request.SchemaJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["schemaJson"] = new[] { "The table schema must be a JSON object with a 'columns' array." },
            }));
        }

        var table = await _db.TableDefinitions
            .Include(t => t.Workspace)
            .FirstOrDefaultAsync(t => t.Id == request.TableId && t.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (table is null)
        {
            return Result.Failure(Error.NotFound($"Table {request.TableId} was not found."));
        }

        table.UpdateSchema(request.SchemaJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidTableSchema(string schemaJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(schemaJson);
            return doc.RootElement.TryGetProperty("columns", out var columnsEl) && columnsEl.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
