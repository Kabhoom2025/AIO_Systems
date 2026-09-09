using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

public class LineageEvent
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public Guid? ParentEventId { get; set; }
    public required string NodeId { get; set; }
    public LineageNodeType NodeType { get; set; }
    public LineageEventType EventType { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public long? DurationMs { get; set; }

    /// <summary>Arbitrary event context (row counts, SQL operation, field values) as JSON.
    /// Sensitive fields (passwords/tokens/secrets) are masked before this is persisted -
    /// see Platform.Lineage's masking policy, never re-mask here.</summary>
    public string? MetadataJson { get; set; }

    public string? ErrorCode { get; set; }

    /// <summary>A sanitized, user-facing error message. Never a raw stack trace.</summary>
    public string? ErrorMessage { get; set; }

    public LineageExecution? Execution { get; set; }
}
