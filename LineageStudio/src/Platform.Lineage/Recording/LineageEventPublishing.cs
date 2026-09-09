using Platform.Domain.Enums;

namespace Platform.Lineage.Recording;

public record LineageEventDto(
    Guid Id,
    Guid ExecutionId,
    Guid ApplicationId,
    Guid? ParentEventId,
    string NodeId,
    LineageNodeType NodeType,
    LineageEventType EventType,
    ExecutionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    long? DurationMs,
    string? MetadataJson,
    string? ErrorCode,
    string? ErrorMessage);

public record LineageExecutionDto(
    Guid Id,
    string CorrelationId,
    Guid ApplicationId,
    Guid VersionId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    ExecutionStatus Status,
    long? DurationMs,
    string? ApplicationName = null);

/// <summary>
/// Fan-out point for "something happened during a lineage-tracked execution" - the recorder
/// always persists to platform.lineage_events first and calls this after, so a live-viewer being
/// down never blocks or loses an execution. The Phase 10 SignalR hub is just another
/// implementation of this registered in place of the no-op default.
/// </summary>
public interface ILineageEventPublisher
{
    Task PublishEventAsync(LineageEventDto evt, CancellationToken ct = default);
    Task PublishExecutionUpdateAsync(LineageExecutionDto execution, CancellationToken ct = default);
}

public class NullLineageEventPublisher : ILineageEventPublisher
{
    public Task PublishEventAsync(LineageEventDto evt, CancellationToken ct = default) => Task.CompletedTask;
    public Task PublishExecutionUpdateAsync(LineageExecutionDto execution, CancellationToken ct = default) => Task.CompletedTask;
}
