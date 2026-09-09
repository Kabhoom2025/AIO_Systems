using System.Globalization;
using System.Text.Json;
using Platform.Domain.Enums;

namespace Platform.Runtime.Execution;

/// <summary>
/// Applies one of the fixed, server-known transformation kinds to a submitted field value. There
/// is deliberately no way to reach this with anything but a TransformationType enum value and a
/// JSON config blob - never a string of user-authored code - matching the Mapping Designer's
/// "no arbitrary JavaScript" rule. Every transformation both takes and returns a string (or null);
/// ColumnValueBinder is the single place that parses that string into the target column's CLR type.
/// </summary>
public static class TransformationEngine
{
    public static string? Apply(
        string? rawValue,
        TransformationType transformation,
        string? configJson,
        IReadOnlyDictionary<string, string?> formData)
    {
        var config = string.IsNullOrWhiteSpace(configJson) ? default : JsonDocument.Parse(configJson).RootElement;

        return transformation switch
        {
            TransformationType.None => rawValue,
            TransformationType.Trim => rawValue?.Trim(),
            TransformationType.Uppercase => rawValue?.ToUpperInvariant(),
            TransformationType.Lowercase => rawValue?.ToLowerInvariant(),
            TransformationType.Default => ApplyDefault(rawValue, config),
            TransformationType.Concatenate => ApplyConcatenate(config, formData),
            TransformationType.Split => ApplySplit(rawValue, config),
            TransformationType.DateConversion => ApplyDateConversion(rawValue, config),
            TransformationType.NumberConversion => ApplyNumberConversion(rawValue, config),
            _ => throw new ArgumentOutOfRangeException(nameof(transformation), transformation, "Unsupported transformation."),
        };
    }

    private static string? ApplyDefault(string? rawValue, JsonElement config)
    {
        if (!string.IsNullOrWhiteSpace(rawValue))
            return rawValue;

        return RequireConfigString(config, "default", TransformationType.Default);
    }

    private static string? ApplyConcatenate(JsonElement config, IReadOnlyDictionary<string, string?> formData)
    {
        if (!config.TryGetProperty("fields", out var fieldsElement) || fieldsElement.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Concatenate requires a transformationConfigJson.fields array of field names.");

        var separator = config.TryGetProperty("separator", out var sep) ? sep.GetString() ?? "" : "";
        var parts = fieldsElement.EnumerateArray()
            .Select(f => formData.GetValueOrDefault(f.GetString() ?? "") ?? "");

        return string.Join(separator, parts);
    }

    private static string? ApplySplit(string? rawValue, JsonElement config)
    {
        if (rawValue is null)
            return null;

        var delimiter = config.TryGetProperty("delimiter", out var d) ? d.GetString() ?? "," : ",";
        var index = config.TryGetProperty("index", out var i) ? i.GetInt32() : 0;

        var parts = rawValue.Split(delimiter);
        if (index < 0 || index >= parts.Length)
            throw new ArgumentException($"Split index {index} is out of range for value '{rawValue}'.");

        return parts[index];
    }

    private static string? ApplyDateConversion(string? rawValue, JsonElement config)
    {
        if (rawValue is null)
            return null;

        var inputFormat = config.TryGetProperty("inputFormat", out var inFmt) ? inFmt.GetString() : null;
        var outputFormat = config.TryGetProperty("outputFormat", out var outFmt) ? outFmt.GetString() : "yyyy-MM-dd";

        var parsed = inputFormat is not null
            ? DateTime.ParseExact(rawValue, inputFormat, CultureInfo.InvariantCulture)
            : DateTime.Parse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None);

        return parsed.ToString(outputFormat, CultureInfo.InvariantCulture);
    }

    private static string? ApplyNumberConversion(string? rawValue, JsonElement config)
    {
        if (rawValue is null)
            return null;

        var cleaned = config.TryGetProperty("stripThousandsSeparator", out var strip) && strip.GetBoolean()
            ? rawValue.Replace(",", "")
            : rawValue;

        if (!decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            throw new ArgumentException($"'{rawValue}' is not a valid number.");

        if (config.TryGetProperty("decimals", out var decimalsElement))
            value = Math.Round(value, decimalsElement.GetInt32());

        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string RequireConfigString(JsonElement config, string property, TransformationType transformation)
    {
        if (config.ValueKind == JsonValueKind.Object && config.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString()!;

        throw new ArgumentException($"{transformation} requires a transformationConfigJson.{property} string.");
    }
}
