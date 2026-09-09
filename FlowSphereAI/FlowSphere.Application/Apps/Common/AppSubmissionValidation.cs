using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlowSphere.Application.Apps.Common;

/// <summary>Shared submission-time checks read out of an app's SettingsJson blob
/// (attributes.uniqueRecord / accessibility.captcha / accessibility.otp), reused by both
/// SubmitAppCommandHandler (authenticated) and SubmitGuestAppRecordCommandHandler (anonymous) so
/// the two submission paths enforce identical rules.</summary>
public static class AppSubmissionValidation
{
    public record UniqueRecordCheck(bool Restricted, bool Warned, string? Message);

    public static UniqueRecordCheck CheckUniqueRecord(string settingsJson, string newDataJson, IEnumerable<string> existingDataJsons)
    {
        var (enabled, type, fieldKeys, message) = ReadUniqueRecordConfig(settingsJson);
        if (!enabled || fieldKeys.Count == 0)
        {
            return new UniqueRecordCheck(false, false, null);
        }

        var newValues = ReadFieldValues(newDataJson, fieldKeys);
        if (newValues.Count == 0)
        {
            return new UniqueRecordCheck(false, false, null);
        }

        var isDuplicate = existingDataJsons
            .Select(json => ReadFieldValues(json, fieldKeys))
            .Any(existing => fieldKeys.All(key => existing.TryGetValue(key, out var v) && newValues.TryGetValue(key, out var nv) && v == nv));

        if (!isDuplicate)
        {
            return new UniqueRecordCheck(false, false, null);
        }

        var resolvedMessage = (message ?? "The '{fields}' must be unique and cannot have duplicates.")
            .Replace("{fields}", string.Join(", ", fieldKeys));

        return type == "Restrict"
            ? new UniqueRecordCheck(true, false, resolvedMessage)
            : new UniqueRecordCheck(false, true, resolvedMessage);
    }

    public static (bool Enabled, string? Token) ReadSharedAccess(string settingsJson, string key)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (!doc.RootElement.TryGetProperty("shared", out var shared))
            {
                return (false, null);
            }

            var enabledKey = key == "guest" ? "guestLinkEnabled" : "webhookEnabled";
            var tokenKey = key == "guest" ? "guestToken" : "webhookApiKey";

            var enabled = shared.TryGetProperty(enabledKey, out var enabledEl) && enabledEl.ValueKind == JsonValueKind.True;
            var token = shared.TryGetProperty(tokenKey, out var tokenEl) && tokenEl.ValueKind == JsonValueKind.String
                ? tokenEl.GetString()
                : null;

            return (enabled, token);
        }
        catch (JsonException)
        {
            return (false, null);
        }
    }

    public static bool IsCaptchaRequiredOnSubmit(string settingsJson) =>
        ReadAccessibilityWhen(settingsJson, "captcha") is "OnSubmission" or "Both";

    public static bool IsOtpRequiredOnSubmit(string settingsJson) =>
        ReadAccessibilityWhen(settingsJson, "otp") is "OnSubmission" or "Both";

    /// <summary>"Email" | "Mobile" | "Both" - which OTP channel(s) the app requires. Defaults to
    /// "Email" (the only channel this session originally shipped) when unset.</summary>
    public static string ReadOtpMethod(string settingsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (!doc.RootElement.TryGetProperty("accessibility", out var accessibility)
                || !accessibility.TryGetProperty("otp", out var otp))
            {
                return "Email";
            }

            return otp.TryGetProperty("method", out var methodEl) && methodEl.ValueKind == JsonValueKind.String
                ? methodEl.GetString() ?? "Email"
                : "Email";
        }
        catch (JsonException)
        {
            return "Email";
        }
    }

    private static (bool Enabled, string Type, List<string> FieldKeys, string? Message) ReadUniqueRecordConfig(string settingsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (!doc.RootElement.TryGetProperty("attributes", out var attributes)
                || !attributes.TryGetProperty("uniqueRecord", out var uniqueRecord))
            {
                return (false, "Warn", new List<string>(), null);
            }

            var enabled = uniqueRecord.TryGetProperty("enabled", out var enabledEl) && enabledEl.ValueKind == JsonValueKind.True;
            var type = uniqueRecord.TryGetProperty("type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String
                ? typeEl.GetString() ?? "Warn"
                : "Warn";
            var message = uniqueRecord.TryGetProperty("message", out var messageEl) && messageEl.ValueKind == JsonValueKind.String
                ? messageEl.GetString()
                : null;

            var fieldKeys = new List<string>();
            if (uniqueRecord.TryGetProperty("fieldKeys", out var keysEl) && keysEl.ValueKind == JsonValueKind.Array)
            {
                fieldKeys.AddRange(keysEl.EnumerateArray().Where(k => k.ValueKind == JsonValueKind.String).Select(k => k.GetString()!));
            }

            return (enabled, type, fieldKeys, message);
        }
        catch (JsonException)
        {
            return (false, "Warn", new List<string>(), null);
        }
    }

    private static string? ReadAccessibilityWhen(string settingsJson, string key)
    {
        try
        {
            using var doc = JsonDocument.Parse(settingsJson);
            if (!doc.RootElement.TryGetProperty("accessibility", out var accessibility)
                || !accessibility.TryGetProperty(key, out var section))
            {
                return null;
            }

            var enabled = section.TryGetProperty("enabled", out var enabledEl) && enabledEl.ValueKind == JsonValueKind.True;
            if (!enabled)
            {
                return null;
            }

            return section.TryGetProperty("when", out var whenEl) && whenEl.ValueKind == JsonValueKind.String
                ? whenEl.GetString()
                : "OnSubmission";
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Dictionary<string, string> ReadFieldValues(string dataJson, IReadOnlyCollection<string> fieldKeys)
    {
        var result = new Dictionary<string, string>();
        var node = JsonNode.Parse(dataJson)?.AsObject();
        if (node is null)
        {
            return result;
        }

        foreach (var key in fieldKeys)
        {
            if (node.TryGetPropertyValue(key, out var value) && value is not null)
            {
                result[key] = value.ToJsonString();
            }
        }

        return result;
    }
}
