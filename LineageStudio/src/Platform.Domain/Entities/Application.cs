namespace Platform.Domain.Entities;

public class AppDefinition
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Screen> Screens { get; set; } = new List<Screen>();
    public ICollection<DataTable> Tables { get; set; } = new List<DataTable>();
    public ICollection<ApiEndpoint> Apis { get; set; } = new List<ApiEndpoint>();
    public ICollection<ServiceDefinition> Services { get; set; } = new List<ServiceDefinition>();
    public ICollection<FieldMapping> Mappings { get; set; } = new List<FieldMapping>();
    public ICollection<ApplicationVersion> Versions { get; set; } = new List<ApplicationVersion>();
}

/// <summary>
/// A publish snapshot of an application's full design-time configuration (screens, components,
/// tables, apis, mappings) so historical lineage executions stay stable even after later edits.
/// </summary>
public class ApplicationVersion
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public int VersionNumber { get; set; }
    public required string SnapshotJson { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
}
