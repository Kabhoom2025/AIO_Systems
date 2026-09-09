using System.Globalization;
using NpgsqlTypes;
using Platform.Domain.Enums;

namespace Platform.Runtime.Execution;

/// <summary>
/// Parses a transformed field value into the CLR type + NpgsqlDbType a bind parameter needs for
/// the target column. Unlike Platform.Infrastructure.DataDesigner.DefaultValueFormatter (which
/// must produce raw SQL text because DDL can't take parameters), runtime DML always goes through
/// NpgsqlParameter, so the safety property here is simpler: never bind a value whose type doesn't
/// actually match the column, and never fall back to a string default that could be
/// misinterpreted - reject anything that fails to parse instead.
/// </summary>
public static class ColumnValueBinder
{
    public static (NpgsqlDbType DbType, object Value) Bind(ColumnDataType dataType, string value)
    {
        return dataType switch
        {
            ColumnDataType.Uuid => (NpgsqlDbType.Uuid, ParseGuid(value)),
            ColumnDataType.Varchar or ColumnDataType.Text => (NpgsqlDbType.Text, value),
            ColumnDataType.Integer => (NpgsqlDbType.Integer, ParseInt(value)),
            ColumnDataType.Bigint => (NpgsqlDbType.Bigint, ParseLong(value)),
            ColumnDataType.Decimal => (NpgsqlDbType.Numeric, ParseDecimal(value)),
            ColumnDataType.Boolean => (NpgsqlDbType.Boolean, ParseBool(value)),
            ColumnDataType.Date => (NpgsqlDbType.Date, ParseDateOnly(value)),
            ColumnDataType.Timestamp => (NpgsqlDbType.Timestamp, ParseDateTime(value)),
            ColumnDataType.Jsonb => (NpgsqlDbType.Jsonb, ParseJson(value)),
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Unsupported column data type."),
        };
    }

    public static NpgsqlDbType DbTypeFor(ColumnDataType dataType) => dataType switch
    {
        ColumnDataType.Uuid => NpgsqlDbType.Uuid,
        ColumnDataType.Varchar or ColumnDataType.Text => NpgsqlDbType.Text,
        ColumnDataType.Integer => NpgsqlDbType.Integer,
        ColumnDataType.Bigint => NpgsqlDbType.Bigint,
        ColumnDataType.Decimal => NpgsqlDbType.Numeric,
        ColumnDataType.Boolean => NpgsqlDbType.Boolean,
        ColumnDataType.Date => NpgsqlDbType.Date,
        ColumnDataType.Timestamp => NpgsqlDbType.Timestamp,
        ColumnDataType.Jsonb => NpgsqlDbType.Jsonb,
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Unsupported column data type."),
    };

    private static Guid ParseGuid(string v) => Guid.TryParse(v, out var g) ? g : throw Invalid(v, "UUID");
    private static int ParseInt(string v) => int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : throw Invalid(v, "integer");
    private static long ParseLong(string v) => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : throw Invalid(v, "bigint");
    private static decimal ParseDecimal(string v) => decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : throw Invalid(v, "decimal");
    private static string ParseJson(string v)
    {
        try
        {
            using var _ = System.Text.Json.JsonDocument.Parse(v);
            return v;
        }
        catch (System.Text.Json.JsonException)
        {
            throw Invalid(v, "JSON");
        }
    }

    private static bool ParseBool(string v) => v.Trim().ToLowerInvariant() switch
    {
        "true" or "1" => true,
        "false" or "0" => false,
        _ => throw Invalid(v, "boolean"),
    };

    private static DateOnly ParseDateOnly(string v) =>
        DateOnly.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : throw Invalid(v, "date");

    private static DateTime ParseDateTime(string v) =>
        DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d)
            ? d
            : throw Invalid(v, "timestamp");

    private static ArgumentException Invalid(string value, string typeName) =>
        new($"Value '{value}' is not a valid {typeName}.");
}
