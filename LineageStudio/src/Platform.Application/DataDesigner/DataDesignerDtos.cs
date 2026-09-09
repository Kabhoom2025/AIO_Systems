using Platform.Domain.Enums;

namespace Platform.Application.DataDesigner;

public record ColumnDto(
    Guid Id,
    Guid TableId,
    string Name,
    ColumnDataType DataType,
    int? Length,
    bool IsPrimaryKey,
    bool IsForeignKey,
    Guid? ReferencesTableId,
    Guid? ReferencesColumnId,
    bool IsUnique,
    bool IsIndexed,
    bool IsNullable,
    string? DefaultValue,
    int OrdinalPosition);

public record TableDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string SchemaName,
    IReadOnlyList<ColumnDto> Columns,
    DateTimeOffset CreatedAt);

public record ColumnDefinition(
    string Name,
    ColumnDataType DataType,
    int? Length = null,
    bool IsPrimaryKey = false,
    bool IsForeignKey = false,
    Guid? ReferencesTableId = null,
    Guid? ReferencesColumnId = null,
    bool IsUnique = false,
    bool IsIndexed = false,
    bool IsNullable = true,
    string? DefaultValue = null);

public record CreateTableRequest(string Name, IReadOnlyList<ColumnDefinition> Columns);

public record AddColumnRequest(ColumnDefinition Column);
