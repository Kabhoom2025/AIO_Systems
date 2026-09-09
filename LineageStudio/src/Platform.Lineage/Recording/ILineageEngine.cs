using Platform.Domain.Enums;

namespace Platform.Lineage.Recording;

public interface ILineageEngine
{
    Task<ILineageExecutionContext> StartExecutionAsync(Guid applicationId, Guid versionId, CancellationToken ct = default);
}

/// <summary>One in-flight execution's recording session. Every event recorded through it shares
/// the same ExecutionId/CorrelationId, so callers never have to thread those through by hand.</summary>
public interface ILineageExecutionContext
{
    Guid ExecutionId { get; }
    string CorrelationId { get; }

    Task<Guid> RecordEventAsync(
        LineageNodeType nodeType,
        string nodeId,
        LineageEventType eventType,
        ExecutionStatus status,
        Guid? parentEventId = null,
        IReadOnlyDictionary<string, object?>? metadata = null,
        string? errorCode = null,
        string? errorMessage = null,
        long? durationMs = null,
        CancellationToken ct = default);

    Task CompleteAsync(ExecutionStatus status, CancellationToken ct = default);
}
