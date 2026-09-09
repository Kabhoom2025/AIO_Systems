using Microsoft.EntityFrameworkCore;
using Platform.Domain.Enums;
using Platform.Infrastructure.Persistence;
using LineageExecutionEntity = Platform.Domain.Entities.LineageExecution;
using LineageEventEntity = Platform.Domain.Entities.LineageEvent;

namespace Platform.Lineage.Recording;

public class LineageEngine : ILineageEngine
{
    private readonly PlatformDbContext _db;
    private readonly ILineageEventPublisher _publisher;
    private readonly MetadataMasker _masker;

    public LineageEngine(PlatformDbContext db, ILineageEventPublisher publisher, MetadataMasker masker)
    {
        _db = db;
        _publisher = publisher;
        _masker = masker;
    }

    public async Task<ILineageExecutionContext> StartExecutionAsync(Guid applicationId, Guid versionId, CancellationToken ct = default)
    {
        var execution = new LineageExecutionEntity
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            ApplicationId = applicationId,
            VersionId = versionId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = ExecutionStatus.Running,
        };

        _db.LineageExecutions.Add(execution);
        await _db.SaveChangesAsync(ct);

        await _publisher.PublishExecutionUpdateAsync(ToDto(execution), ct);

        return new LineageExecutionContext(execution, _db, _publisher, _masker);
    }

    private static LineageExecutionDto ToDto(LineageExecutionEntity e) =>
        new(e.Id, e.CorrelationId, e.ApplicationId, e.VersionId, e.StartedAt, e.CompletedAt, e.Status, e.DurationMs);

    private class LineageExecutionContext : ILineageExecutionContext
    {
        private readonly LineageExecutionEntity _execution;
        private readonly PlatformDbContext _db;
        private readonly ILineageEventPublisher _publisher;
        private readonly MetadataMasker _masker;

        public LineageExecutionContext(LineageExecutionEntity execution, PlatformDbContext db, ILineageEventPublisher publisher, MetadataMasker masker)
        {
            _execution = execution;
            _db = db;
            _publisher = publisher;
            _masker = masker;
        }

        public Guid ExecutionId => _execution.Id;
        public string CorrelationId => _execution.CorrelationId;

        public async Task<Guid> RecordEventAsync(
            LineageNodeType nodeType,
            string nodeId,
            LineageEventType eventType,
            ExecutionStatus status,
            Guid? parentEventId = null,
            IReadOnlyDictionary<string, object?>? metadata = null,
            string? errorCode = null,
            string? errorMessage = null,
            long? durationMs = null,
            CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var evt = new LineageEventEntity
            {
                Id = Guid.NewGuid(),
                ExecutionId = _execution.Id,
                ParentEventId = parentEventId,
                NodeId = nodeId,
                NodeType = nodeType,
                EventType = eventType,
                Status = status,
                StartedAt = now,
                CompletedAt = status is ExecutionStatus.Success or ExecutionStatus.Failed or ExecutionStatus.Skipped ? now : null,
                DurationMs = durationMs,
                MetadataJson = _masker.MaskToJson(metadata),
                ErrorCode = errorCode,
                ErrorMessage = Sanitize(errorMessage),
            };

            _db.LineageEvents.Add(evt);
            await _db.SaveChangesAsync(ct);

            await _publisher.PublishEventAsync(new LineageEventDto(
                evt.Id, evt.ExecutionId, _execution.ApplicationId, evt.ParentEventId, evt.NodeId, evt.NodeType, evt.EventType, evt.Status,
                evt.StartedAt, evt.CompletedAt, evt.DurationMs, evt.MetadataJson, evt.ErrorCode, evt.ErrorMessage), ct);

            return evt.Id;
        }

        public async Task CompleteAsync(ExecutionStatus status, CancellationToken ct = default)
        {
            _execution.CompletedAt = DateTimeOffset.UtcNow;
            _execution.Status = status;
            _execution.DurationMs = (long)(_execution.CompletedAt.Value - _execution.StartedAt).TotalMilliseconds;

            await _db.SaveChangesAsync(ct);
            await _publisher.PublishExecutionUpdateAsync(
                new LineageExecutionDto(_execution.Id, _execution.CorrelationId, _execution.ApplicationId, _execution.VersionId,
                    _execution.StartedAt, _execution.CompletedAt, _execution.Status, _execution.DurationMs), ct);
        }

        /// <summary>Defensive net: even though every call site is expected to pass an
        /// already-sanitized message, never let a raw stack trace slip into stored lineage
        /// data - keep only the first line and cap its length.</summary>
        private static string? Sanitize(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;

            var firstLine = message.Split('\n')[0].Trim();
            return firstLine.Length > 500 ? firstLine[..500] : firstLine;
        }
    }
}
