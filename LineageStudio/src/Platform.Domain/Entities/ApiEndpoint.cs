using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

public class ApiEndpoint
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public ApiHttpMethod Method { get; set; }
    public required string Path { get; set; }
    public string? RequestSchemaJson { get; set; }
    public string? ResponseSchemaJson { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? TableId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
    public ServiceDefinition? Service { get; set; }
    public DataTable? Table { get; set; }
}
