namespace Platform.Domain.Entities;

/// <summary>
/// Metadata describing a real PostgreSQL table generated for a user's application. The actual
/// table lives in the "app_data" schema (never "platform"); this row is what drives its DDL.
/// </summary>
public class DataTable
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public required string Name { get; set; }
    public string SchemaName { get; set; } = "app_data";
    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
    public ICollection<DataColumn> Columns { get; set; } = new List<DataColumn>();
}
