using System.Globalization;
using System.Text.Json;
using Platform.Domain.Enums;

namespace Platform.Infrastructure.DataDesigner;

/// <summary>
/// Turns a user-supplied "default value" string into a SQL literal for a DEFAULT clause.
/// PostgreSQL does not accept bind parameters inside DDL, so this is the only place a value
/// (as opposed to an identifier) flows into DDL text - it never passes the raw input through:
/// every type is round-tripped through .NET's own parser first (rejecting anything that doesn't
/// parse as that exact type) and only the re-serialized, known-safe form is embedded.
/// </summary>
public static class DefaultValueFormatter
{
    public static string ToSqlLiteral(ColumnDataType dataType, string rawValue)
    {
        return dataType switch
        {
            ColumnDataType.Integer => FormatInteger(rawValue),
            ColumnDataType.Bigint => FormatBigint(rawValue),
            ColumnDataType.Decimal => FormatDecimal(rawValue),
            ColumnDataType.Boolean => FormatBoolean(rawValue),
            ColumnDataType.Uuid => FormatUuid(rawValue),
            ColumnDataType.Date => FormatDate(rawValue),
            ColumnDataType.Timestamp => FormatTimestamp(rawValue),
            ColumnDataType.Varchar or ColumnDataType.Text => QuoteStringLiteral(rawValue),
            ColumnDataType.Jsonb => FormatJsonb(rawValue),
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Unsupported column data type."),
        };
    }

    private static string FormatInteger(string raw) =>
        int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value.ToString(CultureInfo.InvariantCulture)
            : throw new ArgumentException($"Default value '{raw}' is not a valid integer.");

    private static string FormatBigint(string raw) =>
        long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value.ToString(CultureInfo.InvariantCulture)
            : throw new ArgumentException($"Default value '{raw}' is not a valid bigint.");

    private static string FormatDecimal(string raw) =>
        decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value.ToString(CultureInfo.InvariantCulture)
            : throw new ArgumentException($"Default value '{raw}' is not a valid decimal.");

    private static string FormatBoolean(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "true" or "1" => "TRUE",
            "false" or "0" => "FALSE",
            _ => throw new ArgumentException($"Default value '{raw}' is not a valid boolean (use true/false)."),
        };

    private static string FormatUuid(string raw) =>
        Guid.TryParse(raw, out var value)
            ? $"'{value}'"
            : throw new ArgumentException($"Default value '{raw}' is not a valid UUID.");

    private static string FormatDate(string raw) =>
        DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? $"'{value:yyyy-MM-dd}'"
            : throw new ArgumentException($"Default value '{raw}' is not a valid date (use yyyy-MM-dd).");

    private static string FormatTimestamp(string raw) =>
        DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var value)
            ? $"'{value:yyyy-MM-dd HH:mm:ss}'"
            : throw new ArgumentException($"Default value '{raw}' is not a valid timestamp.");

    private static string FormatJsonb(string raw)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            var canonical = JsonSerializer.Serialize(document.RootElement);
            return $"{QuoteStringLiteral(canonical)}::jsonb";
        }
        catch (JsonException)
        {
            throw new ArgumentException($"Default value '{raw}' is not valid JSON.");
        }
    }

    private static string QuoteStringLiteral(string raw) => $"'{raw.Replace("'", "''")}'";
}
