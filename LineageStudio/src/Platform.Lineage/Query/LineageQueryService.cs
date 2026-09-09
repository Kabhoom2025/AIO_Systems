using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.Persistence;
using Platform.Lineage.Recording;

namespace Platform.Lineage.Query;

public class LineageQueryService : ILineageQueryService
{
    private readonly PlatformDbContext _db;

    public LineageQueryService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LineageExecutionDto>> ListExecutionsAsync(Guid? applicationId, CancellationToken ct = default)
    {
        var query = _db.LineageExecutions.AsQueryable();
        if (applicationId is Guid appId)
            query = query.Where(e => e.ApplicationId == appId);

        return await query
            .OrderByDescending(e => e.StartedAt)
            .Select(e => new LineageExecutionDto(e.Id, e.CorrelationId, e.ApplicationId, e.VersionId, e.StartedAt, e.CompletedAt, e.Status, e.DurationMs, e.Application!.Name))
            .ToListAsync(ct);
    }

    public async Task<LineageExecutionDto> GetExecutionAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await _db.LineageExecutions.Include(e => e.Application).FirstOrDefaultAsync(e => e.Id == executionId, ct)
            ?? throw new KeyNotFoundException($"Execution '{executionId}' was not found.");

        return new LineageExecutionDto(execution.Id, execution.CorrelationId, execution.ApplicationId, execution.VersionId,
            execution.StartedAt, execution.CompletedAt, execution.Status, execution.DurationMs, execution.Application?.Name);
    }

    public async Task<IReadOnlyList<LineageEventDto>> ListEventsAsync(Guid executionId, CancellationToken ct = default)
    {
        if (!await _db.LineageExecutions.AnyAsync(e => e.Id == executionId, ct))
            throw new KeyNotFoundException($"Execution '{executionId}' was not found.");

        return await _db.LineageEvents
            .Where(e => e.ExecutionId == executionId)
            .OrderBy(e => e.StartedAt)
            .Select(e => new LineageEventDto(e.Id, e.ExecutionId, e.Execution!.ApplicationId, e.ParentEventId, e.NodeId, e.NodeType, e.EventType, e.Status,
                e.StartedAt, e.CompletedAt, e.DurationMs, e.MetadataJson, e.ErrorCode, e.ErrorMessage))
            .ToListAsync(ct);
    }
}
