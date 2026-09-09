using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

/// <summary>
/// A single UI field -> API field -> service field -> database column mapping, as drawn on the
/// React Flow mapping canvas. One API call is usually backed by several of these (one per field).
/// </summary>
public class FieldMapping
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public Guid SourceComponentId { get; set; }
    public required string SourceField { get; set; }

    public Guid ApiId { get; set; }
    public required string ApiField { get; set; }

    public Guid? ServiceId { get; set; }
    public string? ServiceField { get; set; }

    public Guid ColumnId { get; set; }

    public TransformationType Transformation { get; set; } = TransformationType.None;
    public string? TransformationConfigJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public AppDefinition? Application { get; set; }
    public UiComponent? SourceComponent { get; set; }
    public ApiEndpoint? Api { get; set; }
    public ServiceDefinition? Service { get; set; }
    public DataColumn? Column { get; set; }
}
