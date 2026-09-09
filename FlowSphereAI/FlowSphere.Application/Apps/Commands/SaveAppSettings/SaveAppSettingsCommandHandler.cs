using System.Text.Json;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Commands.SaveAppSettings;

public class SaveAppSettingsCommandHandler : IRequestHandler<SaveAppSettingsCommand, Result>
{
    private static readonly HashSet<string> AllowedAppOpenLinkValues = new(StringComparer.Ordinal) { "NewRecord", "AppData" };
    private static readonly HashSet<string> AllowedRedirectModeValues = new(StringComparer.Ordinal)
    {
        "DialogPopup", "PrintRecord", "AppData", "Home", "NewRecord", "ViewRecord", "DownloadRecord",
    };
    private static readonly HashSet<string> AllowedSectionsDisplayValues = new(StringComparer.Ordinal) { "List", "Tabs" };
    private static readonly HashSet<string> AllowedAppUsageValues = new(StringComparer.Ordinal) { "Page", "Views", "Shortcuts", "Webhook", "Triggers" };
    private static readonly HashSet<string> AllowedReadonlyFieldsStyleValues = new(StringComparer.Ordinal) { "WithBorder", "WithoutBorder" };
    private static readonly HashSet<string> AllowedUniqueRecordTypeValues = new(StringComparer.Ordinal) { "Warn", "Restrict" };
    private static readonly HashSet<string> AllowedWhenValues = new(StringComparer.Ordinal) { "BeforeLoad", "OnSubmission", "Both" };
    private static readonly HashSet<string> AllowedOtpMethodValues = new(StringComparer.Ordinal) { "Email", "Mobile", "Both" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public SaveAppSettingsCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result> Handle(SaveAppSettingsCommand request, CancellationToken cancellationToken)
    {
        if (_currentEnvironment.Stage != EnvironmentStage.Dev)
        {
            return Result.Failure(Error.Conflict("Apps can only be edited while in the Dev sandbox stage."));
        }

        if (!IsValidSettings(request.SettingsJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["settingsJson"] = new[] { "The settings must be a JSON object with recognized keys/values for appOpenLink, redirectMode, and sectionsDisplay." },
            }));
        }

        var app = await _db.AppDefinitions
            .Include(a => a.Workspace)
            .FirstOrDefaultAsync(a => a.Id == request.AppId && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage, cancellationToken);

        if (app is null)
        {
            return Result.Failure(Error.NotFound($"App {request.AppId} was not found."));
        }

        app.UpdateSettings(request.SettingsJson);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidSettings(string settingsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("appOpenLink", out var appOpenLink)
                && (appOpenLink.ValueKind != JsonValueKind.String || !AllowedAppOpenLinkValues.Contains(appOpenLink.GetString()!)))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("redirectMode", out var redirectMode)
                && (redirectMode.ValueKind != JsonValueKind.String || !AllowedRedirectModeValues.Contains(redirectMode.GetString()!)))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("sectionsDisplay", out var sectionsDisplay)
                && (sectionsDisplay.ValueKind != JsonValueKind.String || !AllowedSectionsDisplayValues.Contains(sectionsDisplay.GetString()!)))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("appUsage", out var appUsage))
            {
                if (appUsage.ValueKind != JsonValueKind.Array
                    || !appUsage.EnumerateArray().All(e => e.ValueKind == JsonValueKind.String && AllowedAppUsageValues.Contains(e.GetString()!)))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("highlightedFieldKeys", out var highlighted))
            {
                if (highlighted.ValueKind != JsonValueKind.Array || highlighted.GetArrayLength() > 3
                    || !highlighted.EnumerateArray().All(e => e.ValueKind == JsonValueKind.String))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("readonlyFieldsStyle", out var readonlyStyle)
                && (readonlyStyle.ValueKind != JsonValueKind.String || !AllowedReadonlyFieldsStyleValues.Contains(readonlyStyle.GetString()!)))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("customViewFieldKeys", out var customView)
                && (customView.ValueKind != JsonValueKind.Array || !customView.EnumerateArray().All(e => e.ValueKind == JsonValueKind.String)))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("enableComments", out var enableComments) && enableComments.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("notificationsEnabled", out var notificationsEnabled) && notificationsEnabled.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("shared", out var shared) && shared.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("attributes", out var attributes))
            {
                if (attributes.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (attributes.TryGetProperty("uniqueRecord", out var uniqueRecord) && uniqueRecord.ValueKind == JsonValueKind.Object
                    && uniqueRecord.TryGetProperty("type", out var uniqueRecordType)
                    && (uniqueRecordType.ValueKind != JsonValueKind.String || !AllowedUniqueRecordTypeValues.Contains(uniqueRecordType.GetString()!)))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("accessibility", out var accessibility))
            {
                if (accessibility.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var key in new[] { "otp", "captcha" })
                {
                    if (accessibility.TryGetProperty(key, out var section) && section.ValueKind == JsonValueKind.Object
                        && section.TryGetProperty("when", out var whenEl)
                        && (whenEl.ValueKind != JsonValueKind.String || !AllowedWhenValues.Contains(whenEl.GetString()!)))
                    {
                        return false;
                    }
                }

                if (accessibility.TryGetProperty("otp", out var otp) && otp.ValueKind == JsonValueKind.Object
                    && otp.TryGetProperty("method", out var methodEl)
                    && (methodEl.ValueKind != JsonValueKind.String || !AllowedOtpMethodValues.Contains(methodEl.GetString()!)))
                {
                    return false;
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
