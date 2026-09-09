using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Applications;
using Platform.Domain.Enums;
using Platform.Infrastructure.Persistence;
using Platform.Lineage.Recording;
using Platform.Runtime.Execution;

namespace Platform.Lineage.Execution;

/// <summary>
/// Decorates the real runtime engine with lineage recording, without the engine itself knowing
/// lineage exists. It reconstructs a Screen -> Components -> Api -> Transformations -> Service ->
/// Database event chain from (a) metadata it reads itself and (b) the RuntimeExecutionResult the
/// inner engine already returns (which fully accounts for every mapping's raw/transformed value
/// and exactly where execution stopped) - so it never needs to re-implement or peek inside the
/// inner engine's transformation/DML logic.
/// </summary>
public class LineageTrackedRuntimeExecutionService : IRuntimeExecutionService
{
    private readonly RuntimeExecutionService _inner;
    private readonly ILineageEngine _lineage;
    private readonly IApplicationService _applications;
    private readonly PlatformDbContext _db;

    public LineageTrackedRuntimeExecutionService(
        RuntimeExecutionService inner, ILineageEngine lineage, IApplicationService applications, PlatformDbContext db)
    {
        _inner = inner;
        _lineage = lineage;
        _applications = applications;
        _db = db;
    }

    public async Task<RuntimeExecutionResult> ExecuteAsync(Guid applicationId, RuntimeExecuteRequest request, CancellationToken ct = default)
    {
        var app = await _db.Applications.FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new KeyNotFoundException($"Application '{applicationId}' was not found.");

        var versionId = app.CurrentVersionId ?? (await _applications.PublishAsync(applicationId, ct)).Id;

        var api = await _db.Apis.FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Id == request.ApiId, ct)
            ?? throw new KeyNotFoundException($"API '{request.ApiId}' was not found.");

        var mappings = await _db.Mappings.Where(m => m.ApplicationId == applicationId && m.ApiId == api.Id).ToListAsync(ct);

        var components = await _db.Components
            .Include(c => c.Screen)
            .Where(c => mappings.Select(m => m.SourceComponentId).Contains(c.Id))
            .ToListAsync(ct);
        var screen = components.Select(c => c.Screen).FirstOrDefault(s => s is not null);

        var ctx = await _lineage.StartExecutionAsync(applicationId, versionId, ct);

        Guid? screenEventId = null;
        if (screen is not null)
        {
            screenEventId = await ctx.RecordEventAsync(
                LineageNodeType.Screen, $"screen:{screen.Id}", LineageEventType.ScreenStarted, ExecutionStatus.Running,
                metadata: new Dictionary<string, object?> { ["name"] = screen.Name, ["route"] = screen.Route }, ct: ct);

            foreach (var component in components.DistinctBy(c => c.Id))
            {
                await ctx.RecordEventAsync(
                    LineageNodeType.Component, $"component:{component.Id}", LineageEventType.ComponentStarted, ExecutionStatus.Success,
                    parentEventId: screenEventId,
                    metadata: new Dictionary<string, object?> { ["name"] = component.Name, ["type"] = component.Type.ToString() }, ct: ct);
            }
        }

        var apiNodeId = $"api:{api.Id}";
        var apiEventId = await ctx.RecordEventAsync(
            LineageNodeType.Api, apiNodeId, LineageEventType.ApiStarted, ExecutionStatus.Running,
            parentEventId: screenEventId,
            metadata: new Dictionary<string, object?> { ["method"] = api.Method.ToString(), ["path"] = api.Path }, ct: ct);

        var stopwatch = Stopwatch.StartNew();
        var result = await _inner.ExecuteAsync(applicationId, request, ct);
        stopwatch.Stop();

        var service = api.ServiceId is Guid serviceId ? await _db.Services.FirstOrDefaultAsync(s => s.Id == serviceId, ct) : null;
        var table = api.TableId is Guid tableId ? await _db.Tables.FirstOrDefaultAsync(t => t.Id == tableId, ct) : null;

        await RecordTransformationsAsync(ctx, mappings, result, apiEventId, ct);
        var reachedDatabase = result.ErrorCode != "VALIDATION_FAILED";
        await RecordServiceAndDatabaseAsync(ctx, api, service, table, result, apiEventId, reachedDatabase, stopwatch.ElapsedMilliseconds, ct);

        await ctx.RecordEventAsync(
            LineageNodeType.Api, apiNodeId, result.Success ? LineageEventType.ApiCompleted : LineageEventType.ApiFailed,
            result.Success ? ExecutionStatus.Success : ExecutionStatus.Failed,
            parentEventId: apiEventId, errorCode: result.ErrorCode, errorMessage: result.ErrorMessage,
            durationMs: stopwatch.ElapsedMilliseconds, ct: ct);

        var finalStatus = result.Success ? ExecutionStatus.Success : ExecutionStatus.Failed;
        await ctx.RecordEventAsync(
            LineageNodeType.Api, $"execution:{ctx.ExecutionId}", LineageEventType.ExecutionCompleted, finalStatus,
            parentEventId: apiEventId, errorCode: result.ErrorCode, errorMessage: result.ErrorMessage, ct: ct);

        await ctx.CompleteAsync(finalStatus, ct);

        return result;
    }

    /// <summary>A plain read, not an execution - passed straight through with no lineage
    /// recording.</summary>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListTableRowsAsync(
        Guid applicationId, Guid tableId, CancellationToken ct = default)
        => _inner.ListTableRowsAsync(applicationId, tableId, ct);

    /// <summary>A plain read, not an execution - passed straight through with no lineage
    /// recording.</summary>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ListLatestRowsAsync(
        Guid applicationId, Guid tableId, int limit, CancellationToken ct = default)
        => _inner.ListLatestRowsAsync(applicationId, tableId, limit, ct);

    private static async Task RecordTransformationsAsync(
        ILineageExecutionContext ctx, List<Domain.Entities.FieldMapping> mappings, RuntimeExecutionResult result, Guid apiEventId, CancellationToken ct)
    {
        var succeededFields = result.Fields.ToDictionary(f => f.ApiField);
        var failurePointReached = false;

        foreach (var mapping in mappings)
        {
            var nodeId = $"transformation:{mapping.Id}";

            if (succeededFields.TryGetValue(mapping.ApiField, out var field))
            {
                var startId = await ctx.RecordEventAsync(
                    LineageNodeType.Transformation, nodeId, LineageEventType.TransformationStarted, ExecutionStatus.Running,
                    parentEventId: apiEventId,
                    metadata: new Dictionary<string, object?> { ["apiField"] = mapping.ApiField, ["transformation"] = mapping.Transformation.ToString() },
                    ct: ct);

                await ctx.RecordEventAsync(
                    LineageNodeType.Transformation, nodeId, LineageEventType.TransformationCompleted, ExecutionStatus.Success,
                    parentEventId: startId,
                    metadata: new Dictionary<string, object?> { ["rawValue"] = field.RawValue, ["transformedValue"] = field.TransformedValue },
                    ct: ct);
            }
            else if (!failurePointReached && result.ErrorCode == "VALIDATION_FAILED")
            {
                failurePointReached = true;
                await ctx.RecordEventAsync(
                    LineageNodeType.Transformation, nodeId, LineageEventType.ValidationFailed, ExecutionStatus.Failed,
                    parentEventId: apiEventId,
                    metadata: new Dictionary<string, object?> { ["apiField"] = mapping.ApiField },
                    errorCode: result.ErrorCode, errorMessage: result.ErrorMessage, ct: ct);
            }
            else
            {
                await ctx.RecordEventAsync(
                    LineageNodeType.Transformation, nodeId, LineageEventType.TransformationStarted, ExecutionStatus.Skipped,
                    parentEventId: apiEventId,
                    metadata: new Dictionary<string, object?> { ["apiField"] = mapping.ApiField }, ct: ct);
            }
        }
    }

    private static async Task RecordServiceAndDatabaseAsync(
        ILineageExecutionContext ctx, Domain.Entities.ApiEndpoint api, Domain.Entities.ServiceDefinition? service,
        Domain.Entities.DataTable? table, RuntimeExecutionResult result,
        Guid apiEventId, bool reachedDatabase, long elapsedMs, CancellationToken ct)
    {
        Guid? serviceEventId = null;
        var serviceNodeId = api.ServiceId is Guid serviceId ? $"service:{serviceId}" : null;

        if (serviceNodeId is not null)
        {
            serviceEventId = await ctx.RecordEventAsync(
                LineageNodeType.Service, serviceNodeId, LineageEventType.ServiceStarted,
                reachedDatabase ? ExecutionStatus.Running : ExecutionStatus.Skipped,
                parentEventId: apiEventId,
                metadata: service is not null ? new Dictionary<string, object?> { ["name"] = service.Name } : null, ct: ct);
        }

        var databaseNodeId = api.TableId is Guid tableId ? $"table:{tableId}" : "table:unknown";
        var databaseMetadata = table is not null
            ? new Dictionary<string, object?> { ["name"] = table.Name, ["schema"] = table.SchemaName, ["operation"] = api.Method.ToString() }
            : new Dictionary<string, object?> { ["operation"] = api.Method.ToString() };

        if (!reachedDatabase)
        {
            await ctx.RecordEventAsync(
                LineageNodeType.Database, databaseNodeId, LineageEventType.DatabaseStarted, ExecutionStatus.Skipped,
                parentEventId: serviceEventId ?? apiEventId, metadata: databaseMetadata, ct: ct);
            return;
        }

        var dbEventId = await ctx.RecordEventAsync(
            LineageNodeType.Database, databaseNodeId, LineageEventType.DatabaseStarted, ExecutionStatus.Running,
            parentEventId: serviceEventId ?? apiEventId,
            metadata: databaseMetadata, ct: ct);

        await ctx.RecordEventAsync(
            LineageNodeType.Database, databaseNodeId,
            result.Success ? LineageEventType.DatabaseCompleted : LineageEventType.DatabaseFailed,
            result.Success ? ExecutionStatus.Success : ExecutionStatus.Failed,
            parentEventId: dbEventId,
            metadata: result.Success ? new Dictionary<string, object?> { ["rowsReturned"] = result.Rows.Count } : null,
            errorCode: result.Success ? null : result.ErrorCode,
            errorMessage: result.Success ? null : result.ErrorMessage,
            durationMs: elapsedMs, ct: ct);

        if (serviceEventId is not null)
        {
            await ctx.RecordEventAsync(
                LineageNodeType.Service, serviceNodeId!, result.Success ? LineageEventType.ServiceCompleted : LineageEventType.ServiceFailed,
                result.Success ? ExecutionStatus.Success : ExecutionStatus.Failed,
                parentEventId: serviceEventId, errorCode: result.Success ? null : result.ErrorCode,
                errorMessage: result.Success ? null : result.ErrorMessage, ct: ct);
        }
    }
}
