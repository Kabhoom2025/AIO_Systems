using System.Text.Json;
using System.Text.RegularExpressions;
using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Organizations.Commands.SaveOrganizationSettings;

public class SaveOrganizationSettingsCommandHandler : IRequestHandler<SaveOrganizationSettingsCommand, Result>
{
    /// <summary>~6MB image once base64-encoded - no file-storage service exists in this system, so
    /// the background image is embedded directly as a data URI (see Organization.SettingsJson's
    /// doc comment); this cap keeps that blob from growing unreasonably large.</summary>
    private const int MaxDataUriLength = 8_000_000;

    private static readonly HashSet<string> AllowedColorThemeValues = new(StringComparer.Ordinal)
    {
        "default", "ocean", "sunset", "forest", "violet", "rose",
    };

    private static readonly HashSet<string> AllowedFontFamilyValues = new(StringComparer.Ordinal)
    {
        "default", "serif", "rounded", "classic", "mono",
    };

    private const double MinFontScale = 0.875;
    private const double MaxFontScale = 1.25;

    private static readonly string[] CustomColorKeys = { "header", "sidebar", "body" };

    private static readonly Regex HexColorPattern = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public SaveOrganizationSettingsCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SaveOrganizationSettingsCommand request, CancellationToken cancellationToken)
    {
        if (!IsValidSettings(request.SettingsJson))
        {
            return Result.Failure(Error.Validation(new Dictionary<string, string[]>
            {
                ["settingsJson"] = new[] { "The settings must be a JSON object; backgroundImageDataUri (if present) must be a data:image/... URI under ~6MB, colorTheme/fontFamily (if present) must be one of the supported preset names, fontScale (if present) must be between 0.875 and 1.25, and customColors.{header,sidebar,body} (if present) must be #rrggbb hex colors." },
            }));
        }

        var organization = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result.Failure(Error.NotFound("Organization not found."));
        }

        organization.SettingsJson = request.SettingsJson;
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

            if (doc.RootElement.TryGetProperty("backgroundImageDataUri", out var imageEl) && imageEl.ValueKind != JsonValueKind.Null)
            {
                if (imageEl.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                var value = imageEl.GetString() ?? "";
                if (value.Length > MaxDataUriLength || !value.StartsWith("data:image/", StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("glossyThemeEnabled", out var glossyEl) && glossyEl.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                return false;
            }

            if (doc.RootElement.TryGetProperty("colorTheme", out var colorThemeEl) && colorThemeEl.ValueKind != JsonValueKind.Null)
            {
                if (colorThemeEl.ValueKind != JsonValueKind.String || !AllowedColorThemeValues.Contains(colorThemeEl.GetString() ?? ""))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("fontFamily", out var fontFamilyEl) && fontFamilyEl.ValueKind != JsonValueKind.Null)
            {
                if (fontFamilyEl.ValueKind != JsonValueKind.String || !AllowedFontFamilyValues.Contains(fontFamilyEl.GetString() ?? ""))
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("fontScale", out var fontScaleEl) && fontScaleEl.ValueKind != JsonValueKind.Null)
            {
                if (fontScaleEl.ValueKind != JsonValueKind.Number || !fontScaleEl.TryGetDouble(out var fontScale) ||
                    fontScale < MinFontScale || fontScale > MaxFontScale)
                {
                    return false;
                }
            }

            if (doc.RootElement.TryGetProperty("customColors", out var customColorsEl) && customColorsEl.ValueKind != JsonValueKind.Null)
            {
                if (customColorsEl.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var key in CustomColorKeys)
                {
                    if (customColorsEl.TryGetProperty(key, out var colorEl) && colorEl.ValueKind != JsonValueKind.Null)
                    {
                        if (colorEl.ValueKind != JsonValueKind.String || !HexColorPattern.IsMatch(colorEl.GetString() ?? ""))
                        {
                            return false;
                        }
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
