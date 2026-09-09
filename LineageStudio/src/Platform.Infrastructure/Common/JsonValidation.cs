using System.Text.Json;

namespace Platform.Infrastructure.Common;

/// <summary>
/// Every *Json column in platform.* (component properties/validation/events, api request/response
/// schemas, mapping transformation config) is typed jsonb - this is the one place that guards
/// against ever writing a value that isn't actually valid JSON into one of those columns.
/// </summary>
public static class JsonValidation
{
    public static string? ValidateOrNull(string? json, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            return json;
        }
        catch (JsonException)
        {
            throw new ArgumentException($"{fieldName} must be valid JSON.");
        }
    }
}
