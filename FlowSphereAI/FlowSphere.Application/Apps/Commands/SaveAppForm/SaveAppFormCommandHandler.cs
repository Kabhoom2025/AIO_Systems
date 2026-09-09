using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppForm;

public class SaveAppFormCommandHandler : IRequestHandler<SaveAppFormCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppFormCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(SaveAppFormCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidFormSchema(request.FormSchemaJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["formSchemaJson"] = new[] { "The form schema must be a JSON object with a 'fields' array." },
            }));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        app.UpdateFormSchema(request.FormSchemaJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidFormSchema(string formSchemaJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(formSchemaJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            // "fields" is the legacy flat-list shape; "sections" is the current free-form-canvas shape.
            if (doc.RootElement.TryGetProperty("fields", out var fieldsEl) && fieldsEl.ValueKind == JsonValueKind.Array)
            {
                return true;
            }

            return doc.RootElement.TryGetProperty("sections", out var sectionsEl) && sectionsEl.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
