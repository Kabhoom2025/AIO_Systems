namespace Platform.Domain.Entities;

/// <summary>
/// A generated .NET service (business logic layer between an API endpoint and a table) that the
/// runtime engine invokes. Named ServiceDefinition in code to avoid colliding with the many
/// ambient "Service" identifiers in ASP.NET Core DI.
/// </summary>
public class ServiceDefinition
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public required string Name { get; set; }
    public Guid? TableId { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
    public DataTable? Table { get; set; }
}
