using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Platform.Application.DataDesigner;
using Platform.Infrastructure.Persistence;
using DataTableEntity = Platform.Domain.Entities.DataTable;
using DataColumnEntity = Platform.Domain.Entities.DataColumn;

namespace Platform.Infrastructure.DataDesigner;

public class DataTableService : IDataTableService
{
    private readonly PlatformDbContext _db;

    public DataTableService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TableDto>> ListAsync(Guid applicationId, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var tables = await _db.Tables
            .Where(t => t.ApplicationId == applicationId)
            .Include(t => t.Columns)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return tables.Select(ToDto).ToList();
    }

    public async Task<TableDto> GetAsync(Guid applicationId, Guid tableId, CancellationToken ct = default)
    {
        var table = await LoadTableAsync(applicationId, tableId, ct);
        return ToDto(table);
    }

    public async Task<TableDto> CreateAsync(Guid applicationId, CreateTableRequest request, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var tableName = request.Name.Trim();
        SqlIdentifiers.Validate(tableName, "Table name");

        if (await _db.Tables.AnyAsync(t => t.ApplicationId == applicationId && t.Name == tableName, ct))
            throw new InvalidOperationException($"A table named '{tableName}' already exists in this application.");

        if (request.Columns.Count == 0)
            throw new ArgumentException("A table needs at least one column.");

        ValidateColumnNamesUnique(request.Columns);
        foreach (var column in request.Columns)
            SqlIdentifiers.Validate(column.Name, "Column name");

        var schemaName = SqlIdentifiers.SchemaNameFor(applicationId);
        var referenceableTables = await LoadReferenceableTablesAsync(applicationId, request.Columns.Select(c => c.ReferencesTableId), ct);

        var createSchemaSql = DdlBuilder.CreateSchema(schemaName);
        var createTableSql = DdlBuilder.CreateTable(schemaName, tableName, request.Columns, referenceableTables);
        var indexSqls = DdlBuilder.IndexStatements(schemaName, tableName, request.Columns).ToList();

        var now = DateTimeOffset.UtcNow;
        var table = new DataTableEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = tableName,
            SchemaName = schemaName,
            CreatedAt = now,
        };

        var ordinal = 0;
        foreach (var column in request.Columns)
        {
            table.Columns.Add(new DataColumnEntity
            {
                Id = Guid.NewGuid(),
                TableId = table.Id,
                Name = column.Name,
                DataType = column.DataType,
                Length = column.Length,
                IsPrimaryKey = column.IsPrimaryKey,
                IsForeignKey = column.IsForeignKey,
                ReferencesTableId = column.ReferencesTableId,
                ReferencesColumnId = column.ReferencesColumnId,
                IsUnique = column.IsUnique,
                IsIndexed = column.IsIndexed,
                IsNullable = column.IsNullable,
                DefaultValue = column.DefaultValue,
                OrdinalPosition = ordinal++,
            });
        }

        var ddlStatements = new List<string> { createSchemaSql, createTableSql };
        ddlStatements.AddRange(indexSqls);

        await ExecuteWithMetadataAsync(ddlStatements, () =>
        {
            _db.Tables.Add(table);
            return Task.CompletedTask;
        }, ct);

        return ToDto(table);
    }

    public async Task DeleteAsync(Guid applicationId, Guid tableId, CancellationToken ct = default)
    {
        var table = await LoadTableAsync(applicationId, tableId, ct);
        var dropSql = DdlBuilder.DropTable(table.SchemaName, table.Name);

        await ExecuteWithMetadataAsync([dropSql], () =>
        {
            _db.Tables.Remove(table);
            return Task.CompletedTask;
        }, ct);
    }

    public async Task<ColumnDto> AddColumnAsync(Guid applicationId, Guid tableId, AddColumnRequest request, CancellationToken ct = default)
    {
        var table = await LoadTableAsync(applicationId, tableId, ct);
        var column = request.Column;

        var columnName = column.Name.Trim();
        SqlIdentifiers.Validate(columnName, "Column name");

        if (table.Columns.Any(c => c.Name == columnName))
            throw new InvalidOperationException($"Table '{table.Name}' already has a column named '{columnName}'.");

        var normalized = column with { Name = columnName };
        var referenceableTables = await LoadReferenceableTablesAsync(applicationId, [normalized.ReferencesTableId], ct);
        var addColumnSql = DdlBuilder.AddColumn(table.SchemaName, table.Name, normalized, referenceableTables);
        var indexSqls = DdlBuilder.IndexStatements(table.SchemaName, table.Name, [normalized]).ToList();

        var entity = new DataColumnEntity
        {
            Id = Guid.NewGuid(),
            TableId = table.Id,
            Name = normalized.Name,
            DataType = normalized.DataType,
            Length = normalized.Length,
            IsPrimaryKey = normalized.IsPrimaryKey,
            IsForeignKey = normalized.IsForeignKey,
            ReferencesTableId = normalized.ReferencesTableId,
            ReferencesColumnId = normalized.ReferencesColumnId,
            IsUnique = normalized.IsUnique,
            IsIndexed = normalized.IsIndexed,
            IsNullable = normalized.IsNullable,
            DefaultValue = normalized.DefaultValue,
            OrdinalPosition = table.Columns.Count == 0 ? 0 : table.Columns.Max(c => c.OrdinalPosition) + 1,
        };

        var ddlStatements = new List<string> { addColumnSql };
        ddlStatements.AddRange(indexSqls);

        await ExecuteWithMetadataAsync(ddlStatements, () =>
        {
            _db.Columns.Add(entity);
            return Task.CompletedTask;
        }, ct);

        return ToDto(entity);
    }

    public async Task DeleteColumnAsync(Guid applicationId, Guid tableId, Guid columnId, CancellationToken ct = default)
    {
        var table = await LoadTableAsync(applicationId, tableId, ct);
        var column = table.Columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new KeyNotFoundException($"Column '{columnId}' was not found on table '{table.Name}'.");

        var dropSql = DdlBuilder.DropColumn(table.SchemaName, table.Name, column.Name);

        await ExecuteWithMetadataAsync([dropSql], () =>
        {
            _db.Columns.Remove(column);
            return Task.CompletedTask;
        }, ct);
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId, CancellationToken ct)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == applicationId, ct))
            throw new KeyNotFoundException($"Application '{applicationId}' was not found.");
    }

    private async Task<DataTableEntity> LoadTableAsync(Guid applicationId, Guid tableId, CancellationToken ct)
    {
        return await _db.Tables
            .Include(t => t.Columns)
            .FirstOrDefaultAsync(t => t.ApplicationId == applicationId && t.Id == tableId, ct)
            ?? throw new KeyNotFoundException($"Table '{tableId}' was not found.");
    }

    private async Task<IReadOnlyDictionary<Guid, DataTableEntity>> LoadReferenceableTablesAsync(
        Guid applicationId, IEnumerable<Guid?> referencedTableIds, CancellationToken ct)
    {
        var ids = referencedTableIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, DataTableEntity>();

        var tables = await _db.Tables
            .Where(t => t.ApplicationId == applicationId && ids.Contains(t.Id))
            .Include(t => t.Columns)
            .ToListAsync(ct);

        return tables.ToDictionary(t => t.Id);
    }

    private static void ValidateColumnNamesUnique(IReadOnlyList<ColumnDefinition> columns)
    {
        var duplicate = columns
            .GroupBy(c => c.Name.Trim(), StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
            throw new ArgumentException($"Column name '{duplicate.Key}' is used more than once.");
    }

    /// <summary>Runs the given DDL statements and the metadata mutation under one transaction, so a
    /// rejected schema change (a real Postgres error) never leaves platform.* metadata out of sync
    /// with what actually exists in the database.</summary>
    private async Task ExecuteWithMetadataAsync(IReadOnlyList<string> ddlStatements, Func<Task> applyMetadata, CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var connection = (NpgsqlConnection)_db.Database.GetDbConnection();
            var dbTransaction = (NpgsqlTransaction)transaction.GetDbTransaction();

            foreach (var sql in ddlStatements)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Transaction = dbTransaction;
                await command.ExecuteNonQueryAsync(ct);
            }

            await applyMetadata();
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (PostgresException pex)
        {
            await transaction.RollbackAsync(ct);
            throw new InvalidOperationException($"The database rejected this schema change: {pex.MessageText}");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static TableDto ToDto(DataTableEntity table) => new(
        table.Id,
        table.ApplicationId,
        table.Name,
        table.SchemaName,
        table.Columns.OrderBy(c => c.OrdinalPosition).Select(ToDto).ToList(),
        table.CreatedAt);

    private static ColumnDto ToDto(DataColumnEntity column) => new(
        column.Id,
        column.TableId,
        column.Name,
        column.DataType,
        column.Length,
        column.IsPrimaryKey,
        column.IsForeignKey,
        column.ReferencesTableId,
        column.ReferencesColumnId,
        column.IsUnique,
        column.IsIndexed,
        column.IsNullable,
        column.DefaultValue,
        column.OrdinalPosition);
}
