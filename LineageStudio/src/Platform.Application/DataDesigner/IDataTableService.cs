namespace Platform.Application.DataDesigner;

/// <summary>
/// Backs the Data Designer. Every mutating method here does two things atomically: it updates
/// platform.tables/platform.columns metadata AND executes the matching real DDL (CREATE TABLE,
/// ALTER TABLE ADD/DROP COLUMN, DROP TABLE) against the application's own PostgreSQL schema, so
/// metadata never drifts from what actually exists in the database.
/// </summary>
public interface IDataTableService
{
    Task<IReadOnlyList<TableDto>> ListAsync(Guid applicationId, CancellationToken ct = default);
    Task<TableDto> GetAsync(Guid applicationId, Guid tableId, CancellationToken ct = default);
    Task<TableDto> CreateAsync(Guid applicationId, CreateTableRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid tableId, CancellationToken ct = default);
    Task<ColumnDto> AddColumnAsync(Guid applicationId, Guid tableId, AddColumnRequest request, CancellationToken ct = default);
    Task DeleteColumnAsync(Guid applicationId, Guid tableId, Guid columnId, CancellationToken ct = default);
}
