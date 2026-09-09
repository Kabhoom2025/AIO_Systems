using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppAccessPermissions;

public class SaveAppAccessPermissionsCommandHandler : IRequestHandler<SaveAppAccessPermissionsCommand, Result>
{
    private static readonly HashSet<string> AllowedStates = new(StringComparer.Ordinal) { "Editable", "Readonly", "Hidden", "Custom" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppAccessPermissionsCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(SaveAppAccessPermissionsCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidAccessPermissions(request.AccessPermissionsJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["accessPermissionsJson"] = new[] { "The access permissions must be a JSON object mapping workflow step keys to an object of field keys mapped to 'Editable', 'Readonly', 'Hidden', or 'Custom'." },
            }));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        app.UpdateAccessPermissions(request.AccessPermissionsJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>Root is { stepKey: { fieldKey: "Editable"|"Readonly"|"Hidden"|"Custom" } } - a step's
    /// leaf map may be empty (no fields configured for it yet) but every leaf value must be one of
    /// the allowed states.</summary>
    private static bool IsValidAccessPermissions(string accessPermissionsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(accessPermissionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var stepProperty in doc.RootElement.EnumerateObject())
            {
                if (stepProperty.Value.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var fieldProperty in stepProperty.Value.EnumerateObject())
                {
                    if (fieldProperty.Value.ValueKind != JsonValueKind.String || !AllowedStates.Contains(fieldProperty.Value.GetString()!))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
