using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using Platform.Domain.Entities;
using Platform.Domain.Enums;
using Platform.Infrastructure.DataDesigner;
using Platform.Infrastructure.Persistence;

namespace Platform.Runtime.Execution;

/// <summary>
/// Actually runs a generated application's API: resolves the mappings for the requested API,
/// applies each field's transformation, and executes real parameterized DML against the
/// application's own PostgreSQL schema (never platform.* metadata). A rejected constraint (e.g. a
/// duplicate email) comes back as a normal RuntimeExecutionResult with Success = false rather than
/// an exception, since that is an expected FAILED outcome the caller should render, not a server
/// error - misconfiguration (API/table/mapping doesn't exist or doesn't line up) still throws.
/// </summary>
public class RuntimeExecutionService : IRuntimeExecutionService
{
    private readonly PlatformDbContext _db;
    private readonly string _connectionString;

    public RuntimeExecutionService(PlatformDbContext db, IConfiguration configuration)
    {
        _db = db;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task<RuntimeExecutionResult> ExecuteAsync(Guid applicationId, RuntimeExecuteRequest request, CancellationToken ct = default)
    {
        var api = await _db.Apis.FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Id == request.ApiId, ct)
            ?? throw new KeyNotFoundException($"API '{request.ApiId}' was not found.");

        if (api.TableId is not Guid tableId)
            throw new InvalidOperationException("This API is not linked to a database table.");

        var table = await _db.Tables.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == tableId, ct)
            ?? throw new KeyNotFoundException($"Table '{tableId}' was not found.");

        var mappings = await _db.Mappings
            .Where(m => m.ApplicationId == applicationId && m.ApiId == api.Id)
            .ToListAsync(ct);

        foreach (var mapping in mappings)
        {
            if (table.Columns.All(c => c.Id != mapping.ColumnId))
                throw new InvalidOperationException($"Mapping '{mapping.Id}' targets a column outside table '{table.Name}'.");
        }

        var formDataAsStrings = request.FormData.ToDictionary(kv => kv.Key, kv => ElementToString(kv.Value));

        var fields = new List<RuntimeFieldResult>();
        var columnValues = new Dictionary<Guid, string?>();

        try
        {
            foreach (var mapping in mappings)
            {
                var column = table.Columns.First(c => c.Id == mapping.ColumnId);
                formDataAsStrings.TryGetValue(mapping.ApiField, out var raw);
                var transformed = TransformationEngine.Apply(raw, mapping.Transformation, mapping.TransformationConfigJson, formDataAsStrings);

                fields.Add(new RuntimeFieldResult(mapping.ApiField, column.Name, raw, transformed));
                columnValues[column.Id] = transformed;
            }
        }
        catch (ArgumentException ex)
        {
            return new RuntimeExecutionResult(false, "VALIDATION_FAILED", ex.Message, fields, []);
        }

        try
        {
            return api.Method switch
            {
                ApiHttpMethod.Post => await ExecuteInsertAsync(table, columnValues, fields, ct),
                ApiHttpMethod.Get => await ExecuteSelectAsync(table, ct),
                ApiHttpMethod.Put or ApiHttpMethod.Patch => await ExecuteUpdateAsync(table, columnValues, fields, ct),
                ApiHttpMethod.Delete => await ExecuteDeleteAsync(table, columnValues, fields, ct),
                _ => throw new ArgumentOutOfRangeException(nameof(api.Method), api.Method, "Unsupported HTTP method."),
            };
        }
        catch (ArgumentException ex)
        {
            return new RuntimeExecutionResult(false, "VALIDATION_FAILED", ex.Message, fields, []);
        }
        catch (PostgresException pex) when (pex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new RuntimeExecutionResult(false, "DUPLICATE_KEY", pex.MessageText, fields, []);
        }
        catch (PostgresException pex) when (pex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return new RuntimeExecutionResult(false, "FOREIGN_KEY_VIOLATION", pex.MessageText, fields, []);
        }
        catch (PostgresException pex) when (pex.SqlState == PostgresErrorCodes.NotNullViolation)
        {
            return new RuntimeExecutionResult(false, "VALIDATION_FAILED", pex.MessageText, fields, []);
        }
        catch (PostgresException pex)
        {
            return new RuntimeExecutionResult(false, "DATABASE_ERROR", pex.MessageText, fields, []);
        }
    }

    private async Task<RuntimeExecutionResult> ExecuteInsertAsync(
        DataTable table, Dictionary<Guid, string?> columnValues, List<RuntimeFieldResult> fields, CancellationToken ct)
    {
        var columnNames = new List<string>();
        var valueSql = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        var paramIndex = 0;

        foreach (var column in table.Columns.OrderBy(c => c.OrdinalPosition))
        {
            var hasValue = columnValues.TryGetValue(column.Id, out var value) && value is not null;

            if (!hasValue)
            {
                if (column.IsPrimaryKey && column.DataType == ColumnDataType.Uuid)
                {
                    columnNames.Add(SqlIdentifiers.Quote(column.Name, "Column name"));
                    valueSql.Add("gen_random_uuid()");
                    continue;
                }

                if (!column.IsNullable && column.DefaultValue is null)
                    throw new ArgumentException($"'{column.Name}' is required.");

                continue;
            }

            var paramName = $"p{paramIndex++}";
            var (dbType, bound) = ColumnValueBinder.Bind(column.DataType, value!);
            parameters.Add(new NpgsqlParameter(paramName, dbType) { Value = bound });

            columnNames.Add(SqlIdentifiers.Quote(column.Name, "Column name"));
            valueSql.Add($"@{paramName}");
        }

        var sql = $"INSERT INTO {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} " +
                  $"({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", valueSql)}) RETURNING *;";

        var rows = await ExecuteReturningAsync(sql, parameters, ct);
        return new RuntimeExecutionResult(true, null, null, fields, rows);
    }

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListTableRowsAsync(
        Guid applicationId, Guid tableId, CancellationToken ct = default)
    {
        var table = await _db.Tables.FirstOrDefaultAsync(t => t.ApplicationId == applicationId && t.Id == tableId, ct)
            ?? throw new KeyNotFoundException($"Table '{tableId}' was not found.");

        var sql = $"SELECT * FROM {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} LIMIT 200;";
        return await ExecuteReturningAsync(sql, [], ct);
    }

    /// <summary>Most-recently-written rows first, for a "what just flowed in" viewer. Generated
    /// tables carry no created-at column of their own, so this orders by ctid (the row's physical
    /// heap location) instead - for a table that's only ever appended to, never updated in place,
    /// that reliably tracks insertion order without needing every table to be schema-changed for
    /// it. It stops being reliable across a table that gets rewritten (VACUUM FULL, updates), but
    /// this is a "recently flowed in" viewer, not an audit log.</summary>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListLatestRowsAsync(
        Guid applicationId, Guid tableId, int limit, CancellationToken ct = default)
    {
        var table = await _db.Tables.FirstOrDefaultAsync(t => t.ApplicationId == applicationId && t.Id == tableId, ct)
            ?? throw new KeyNotFoundException($"Table '{tableId}' was not found.");

        var boundedLimit = Math.Clamp(limit, 1, 200);
        var sql = $"SELECT * FROM {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} ORDER BY ctid DESC LIMIT @limit;";
        var parameters = new List<NpgsqlParameter> { new("limit", boundedLimit) };
        return await ExecuteReturningAsync(sql, parameters, ct);
    }

    private async Task<RuntimeExecutionResult> ExecuteSelectAsync(DataTable table, CancellationToken ct)
    {
        var sql = $"SELECT * FROM {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} LIMIT 100;";
        var rows = await ExecuteReturningAsync(sql, [], ct);
        return new RuntimeExecutionResult(true, null, null, [], rows);
    }

    private async Task<RuntimeExecutionResult> ExecuteUpdateAsync(
        DataTable table, Dictionary<Guid, string?> columnValues, List<RuntimeFieldResult> fields, CancellationToken ct)
    {
        var pkColumn = table.Columns.FirstOrDefault(c => c.IsPrimaryKey)
            ?? throw new InvalidOperationException($"Table '{table.Name}' has no primary key.");

        if (!columnValues.TryGetValue(pkColumn.Id, out var pkValue) || pkValue is null)
            throw new ArgumentException($"'{pkColumn.Name}' (the primary key) is required to update a row.");

        var setClauses = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        var paramIndex = 0;

        foreach (var column in table.Columns.Where(c => !c.IsPrimaryKey))
        {
            if (!columnValues.TryGetValue(column.Id, out var value))
                continue;

            var paramName = $"p{paramIndex++}";
            if (value is null)
            {
                setClauses.Add($"{SqlIdentifiers.Quote(column.Name, "Column name")} = NULL");
                continue;
            }

            var (dbType, bound) = ColumnValueBinder.Bind(column.DataType, value);
            parameters.Add(new NpgsqlParameter(paramName, dbType) { Value = bound });
            setClauses.Add($"{SqlIdentifiers.Quote(column.Name, "Column name")} = @{paramName}");
        }

        if (setClauses.Count == 0)
            throw new ArgumentException("No fields to update were provided.");

        var (pkDbType, pkBound) = ColumnValueBinder.Bind(pkColumn.DataType, pkValue);
        var pkParam = new NpgsqlParameter("pk", pkDbType) { Value = pkBound };
        parameters.Add(pkParam);

        var sql = $"UPDATE {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} " +
                  $"SET {string.Join(", ", setClauses)} WHERE {SqlIdentifiers.Quote(pkColumn.Name, "Column name")} = @pk RETURNING *;";

        var rows = await ExecuteReturningAsync(sql, parameters, ct);
        if (rows.Count == 0)
            return new RuntimeExecutionResult(false, "NOT_FOUND", $"No row with {pkColumn.Name} = {pkValue} was found.", fields, []);

        return new RuntimeExecutionResult(true, null, null, fields, rows);
    }

    private async Task<RuntimeExecutionResult> ExecuteDeleteAsync(
        DataTable table, Dictionary<Guid, string?> columnValues, List<RuntimeFieldResult> fields, CancellationToken ct)
    {
        var pkColumn = table.Columns.FirstOrDefault(c => c.IsPrimaryKey)
            ?? throw new InvalidOperationException($"Table '{table.Name}' has no primary key.");

        if (!columnValues.TryGetValue(pkColumn.Id, out var pkValue) || pkValue is null)
            throw new ArgumentException($"'{pkColumn.Name}' (the primary key) is required to delete a row.");

        var (pkDbType, pkBound) = ColumnValueBinder.Bind(pkColumn.DataType, pkValue);
        var pkParam = new NpgsqlParameter("pk", pkDbType) { Value = pkBound };

        var sql = $"DELETE FROM {SqlIdentifiers.QualifiedTable(table.SchemaName, table.Name)} " +
                  $"WHERE {SqlIdentifiers.Quote(pkColumn.Name, "Column name")} = @pk RETURNING *;";

        var rows = await ExecuteReturningAsync(sql, [pkParam], ct);
        if (rows.Count == 0)
            return new RuntimeExecutionResult(false, "NOT_FOUND", $"No row with {pkColumn.Name} = {pkValue} was found.", fields, []);

        return new RuntimeExecutionResult(true, null, null, fields, rows);
    }

    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExecuteReturningAsync(
        string sql, IReadOnlyList<NpgsqlParameter> parameters, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameter in parameters)
            command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<IReadOnlyDictionary<string, object?>>();

        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                row[reader.GetName(i)] = value is DBNull ? null : value;
            }
            rows.Add(row);
        }

        return rows;
    }

    private static string? ElementToString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => element.GetRawText(),
    };
}
