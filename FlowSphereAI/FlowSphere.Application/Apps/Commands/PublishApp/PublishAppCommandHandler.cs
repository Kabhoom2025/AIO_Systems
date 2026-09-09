using System.Text.Json;
using FlowSphere.Application.Common;
using System.Linq;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.PublishApp;

public class PublishAppCommandHandler : IRequestHandler<PublishAppCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public PublishAppCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(PublishAppCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be published from the Dev sandbox stage."));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        if (!HasAtLeastOneField(app.FormSchemaJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["form"] = new[] { "Add at least one field to the form before publishing." },
            }));
        }

        app.Publish();

        var publisherName = await _db.Users
            .Where(u => u.Id == _currentUser.UserId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "Unknown user";

        _db.AppPublishHistoryEntries.Add(new Domain.Entities.AppPublishHistoryEntry
        {
            OrganizationId = _currentUser.OrganizationId,
            AppId = app.Id,
            Comment = request.Comment,
            PublishedByUserId = _currentUser.UserId,
            PublishedByUserName = publisherName,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool HasAtLeastOneField(string formSchemaJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(formSchemaJson);
            var root = doc.RootElement;

            // Legacy shape: a flat top-level "fields" array.
            if (root.TryGetProperty("fields", out var fieldsEl) && fieldsEl.ValueKind == JsonValueKind.Array)
            {
                return fieldsEl.GetArrayLength() > 0;
            }

            // Current shape: fields nested under sections.
            if (root.TryGetProperty("sections", out var sectionsEl) && sectionsEl.ValueKind == JsonValueKind.Array)
            {
                return sectionsEl.EnumerateArray().Any(section =>
                    section.TryGetProperty("fields", out var sectionFieldsEl)
                    && sectionFieldsEl.ValueKind == JsonValueKind.Array
                    && sectionFieldsEl.GetArrayLength() > 0);
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
