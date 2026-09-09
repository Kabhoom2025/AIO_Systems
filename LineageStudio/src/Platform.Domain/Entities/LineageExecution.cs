using Platform.Domain.Enums;

namespace Platform.Domain.Entities;

public class LineageExecution
{
    public Guid Id { get; set; }
    public required string CorrelationId { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid VersionId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public long? DurationMs { get; set; }

    public AppDefinition? Application { get; set; }
    public ApplicationVersion? Version { get; set; }
    public ICollection<LineageEvent> Events { get; set; } = new List<LineageEvent>();
}
