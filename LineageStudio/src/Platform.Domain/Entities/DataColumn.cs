using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

public class DataColumn
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public required string Name { get; set; }
    public ColumnDataType DataType { get; set; }
    public int? Length { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public Guid? ReferencesTableId { get; set; }
    public Guid? ReferencesColumnId { get; set; }
    public bool IsUnique { get; set; }
    public bool IsIndexed { get; set; }
    public bool IsNullable { get; set; } = true;
    public string? DefaultValue { get; set; }
    public int OrdinalPosition { get; set; }

    public DataTable? Table { get; set; }
}
