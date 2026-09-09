using System.Text;
using Platform.Application.DataDesigner;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.DataDesigner;

/// <summary>
/// Builds DDL text purely from already-validated, already-quoted pieces (SqlIdentifiers.Quote,
/// ColumnTypeMapper, DefaultValueFormatter) - it never accepts a raw, unvalidated string.
/// </summary>
public static class DdlBuilder
{
    public static string CreateSchema(string schemaName) =>
        $"CREATE SCHEMA IF NOT EXISTS {SqlIdentifiers.Quote(schemaName, "Schema name")};";

    /// <summary>Drops an application's entire real schema (every table it ever created, in one
    /// go) - used when the application itself is deleted, so nothing is orphaned behind in
    /// Postgres with no metadata left pointing at it.</summary>
    public static string DropSchema(string schemaName) =>
        $"DROP SCHEMA IF EXISTS {SqlIdentifiers.Quote(schemaName, "Schema name")} CASCADE;";

    public static string DropTable(string schemaName, string tableName) =>
        $"DROP TABLE IF EXISTS {SqlIdentifiers.QualifiedTable(schemaName, tableName)};";

    public static string DropColumn(string schemaName, string tableName, string columnName) =>
        $"ALTER TABLE {SqlIdentifiers.QualifiedTable(schemaName, tableName)} " +
        $"DROP COLUMN {SqlIdentifiers.Quote(columnName, "Column name")};";

    public static string CreateTable(
        string schemaName,
        string tableName,
        IReadOnlyList<ColumnDefinition> columns,
        IReadOnlyDictionary<Guid, DataTable> referenceableTables)
    {
        var lines = columns.Select(c => "    " + ColumnDefinitionSql(c, referenceableTables)).ToList();

        var primaryKeyColumns = columns.Where(c => c.IsPrimaryKey).Select(c => SqlIdentifiers.Quote(c.Name, "Column name")).ToList();
        if (primaryKeyColumns.Count > 0)
            lines.Add($"    PRIMARY KEY ({string.Join(", ", primaryKeyColumns)})");

        var body = string.Join(",\n", lines);
        return $"CREATE TABLE {SqlIdentifiers.QualifiedTable(schemaName, tableName)} (\n{body}\n);";
    }

    public static string AddColumn(
        string schemaName,
        string tableName,
        ColumnDefinition column,
        IReadOnlyDictionary<Guid, DataTable> referenceableTables)
    {
        return $"ALTER TABLE {SqlIdentifiers.QualifiedTable(schemaName, tableName)} " +
               $"ADD COLUMN {ColumnDefinitionSql(column, referenceableTables)};";
    }

    public static IEnumerable<string> IndexStatements(string schemaName, string tableName, IReadOnlyList<ColumnDefinition> columns)
    {
        foreach (var column in columns)
        {
            // PRIMARY KEY and UNIQUE constraints already create their own index; only a plain
            // IsIndexed request (not also PK/unique) needs an explicit CREATE INDEX.
            if (column.IsIndexed && !column.IsPrimaryKey && !column.IsUnique)
            {
                var quotedTable = SqlIdentifiers.Quote(tableName, "Table name");
                var quotedColumn = SqlIdentifiers.Quote(column.Name, "Column name");
                yield return $"CREATE INDEX ON {SqlIdentifiers.QualifiedTable(schemaName, tableName)} ({quotedColumn});";
            }
        }
    }

    private static string ColumnDefinitionSql(ColumnDefinition column, IReadOnlyDictionary<Guid, DataTable> referenceableTables)
    {
        var sb = new StringBuilder();
        sb.Append(SqlIdentifiers.Quote(column.Name, "Column name"));
        sb.Append(' ');
        sb.Append(ColumnTypeMapper.ToSqlType(column.DataType, column.Length));

        if (!column.IsNullable)
            sb.Append(" NOT NULL");

        if (column.IsUnique)
            sb.Append(" UNIQUE");

        if (!string.IsNullOrWhiteSpace(column.DefaultValue))
        {
            sb.Append(" DEFAULT ");
            sb.Append(DefaultValueFormatter.ToSqlLiteral(column.DataType, column.DefaultValue));
        }

        if (column.IsForeignKey)
        {
            if (column.ReferencesTableId is null || column.ReferencesColumnId is null)
                throw new ArgumentException($"Column '{column.Name}' is marked as a foreign key but has no reference target.");

            if (!referenceableTables.TryGetValue(column.ReferencesTableId.Value, out var referencedTable))
                throw new ArgumentException($"Column '{column.Name}' references an unknown table.");

            var referencedColumn = referencedTable.Columns.FirstOrDefault(c => c.Id == column.ReferencesColumnId.Value)
                ?? throw new ArgumentException($"Column '{column.Name}' references an unknown column.");

            sb.Append(" REFERENCES ");
            sb.Append(SqlIdentifiers.QualifiedTable(referencedTable.SchemaName, referencedTable.Name));
            sb.Append(" (");
            sb.Append(SqlIdentifiers.Quote(referencedColumn.Name, "Column name"));
            sb.Append(')');
        }

        return sb.ToString();
    }
}
