using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.LinkAppTable;

public class LinkAppTableCommandHandler : IRequestHandler<LinkAppTableCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public LinkAppTableCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(LinkAppTableCommand request, CancellationToken cancellationToken)
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

        if (request.TableId is not null)
        {
            var tableExists = await _db.TableDefinitions
                .AnyAsync(t => t.Id == request.TableId && t.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

            if (!tableExists)
            {
                return Result.Failure(Error.NotFound($"Table {request.TableId} was not found."));
            }

            if (!IsValidJsonObject(request.FieldMappingJson))
            {
                return Result.Failure(Error.Validation(new Dictionary<string, string[]>
                {
                    ["fieldMappingJson"] = new[] { "The field mapping must be a JSON object." },
                }));
            }
        }

        app.LinkTable(request.TableId, request.FieldMappingJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidJsonObject(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
