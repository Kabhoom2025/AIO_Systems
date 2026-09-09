using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppTrigger;

public class SaveAppTriggerCommandHandler : IRequestHandler<SaveAppTriggerCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppTriggerCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<int>> Handle(SaveAppTriggerCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result<int>.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidFieldMappingJson(request.FieldMappingJson))
        {
            return Result<int>.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["fieldMappingJson"] = new[] { "Field mapping must be a JSON object." },
            }));
        }

        var sourceExists = await _db.AppDefinitions
            .AnyAsync(a => a.Id == request.SourceAppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);
        if (!sourceExists)
        {
            return Result<int>.Failure(Error.NotFound($"Source app {request.SourceAppId} was not found."));
        }

        var destinationExists = await _db.AppDefinitions
            .AnyAsync(a => a.Id == request.DestinationAppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);
        if (!destinationExists)
        {
            return Result<int>.Failure(Error.NotFound($"Destination app {request.DestinationAppId} was not found."));
        }

        var trigger = request.Id is null
            ? new AppTrigger { OrganizationId = _currentUser.OrganizationId, SourceAppId = request.SourceAppId }
            : await _db.AppTriggers.FirstOrDefaultAsync(t => t.Id == request.Id && t.SourceAppId == request.SourceAppId, cancellationToken);

        if (trigger is null)
        {
            return Result<int>.Failure(Error.NotFound($"Trigger {request.Id} was not found."));
        }

        trigger.Name = request.Name;
        trigger.DestinationAppId = request.DestinationAppId;
        trigger.ActionType = request.ActionType;
        trigger.FieldMappingJson = request.FieldMappingJson;
        trigger.IsEnabled = request.IsEnabled;

        if (request.Id is null)
        {
            _db.AppTriggers.Add(trigger);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(trigger.Id);
    }

    private static bool IsValidFieldMappingJson(string json)
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
