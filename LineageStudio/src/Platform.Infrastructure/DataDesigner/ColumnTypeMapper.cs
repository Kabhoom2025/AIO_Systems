using Platform.Domain.Enums;

namespace Platform.Infrastructure.DataDesigner;

public static class ColumnTypeMapper
{
    private const int DefaultVarcharLength = 255;

    public static string ToSqlType(ColumnDataType dataType, int? length)
    {
        return dataType switch
        {
            ColumnDataType.Uuid => "uuid",
            ColumnDataType.Varchar => $"varchar({ValidateLength(length ?? DefaultVarcharLength)})",
            ColumnDataType.Text => "text",
            ColumnDataType.Integer => "integer",
            ColumnDataType.Bigint => "bigint",
            ColumnDataType.Decimal => length is int precision ? $"numeric({ValidateLength(precision)})" : "numeric",
            ColumnDataType.Boolean => "boolean",
            ColumnDataType.Date => "date",
            ColumnDataType.Timestamp => "timestamp",
            ColumnDataType.Jsonb => "jsonb",
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Unsupported column data type."),
        };
    }

    private static int ValidateLength(int length)
    {
        if (length is < 1 or > 10485760)
            throw new ArgumentException($"Column length {length} is out of range.");
        return length;
    }
}
