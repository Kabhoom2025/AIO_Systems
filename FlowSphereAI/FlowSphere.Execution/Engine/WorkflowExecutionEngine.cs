using System.Text.Json;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowSphere.Execution.Engine;

/// <summary>
/// Orchestrates a single workflow run: loads the graph, walks it node-by-node starting from
/// Trigger (or resumes mid-graph after a User Task is approved/rejected), and persists an
/// ExecutionStepLog row after EVERY step (not batched) so a running/paused/failed execution is
/// fully inspectable mid-flight via GetExecutionDetail.
/// </summary>
public class WorkflowExecutionEngine
{
    private readonly IApplicationDbContext _db;
    private readonly INodeExecutorRegistry _nodeExecutorRegistry;
    private readonly IExecutionNotifier _notifier;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WorkflowExecutionEngine> _logger;

    public WorkflowExecutionEngine(
        IApplicationDbContext db,
        INodeExecutorRegistry nodeExecutorRegistry,
        IExecutionNotifier notifier,
        IServiceScopeFactory scopeFactory,
        ILogger<WorkflowExecutionEngine> logger)
    {
        _db = db;
        _nodeExecutorRegistry = nodeExecutorRegistry;
        _notifier = notifier;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task RunAsync(Guid executionId, CancellationToken cancellationToken)
    {
        var execution = await _db.WorkflowExecutions
            .IgnoreQueryFilters() // background dispatcher has no HTTP-request tenant context
            .FirstOrDefaultAsync(e => e.Id == executionId, cancellationToken);

        if (execution is null)
        {
            _logger.LogWarning("Execution {ExecutionId} was not found - skipping.", executionId);
            return;
        }

        var version = await _db.WorkflowVersions
            .FirstOrDefaultAsync(v => v.Id == execution.WorkflowVersionId, cancellationToken);

        if (version is null)
        {
            execution.MarkFailed("The workflow version for this execution no longer exists.");
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var graph = WorkflowGraph.Parse(version.GraphJson);
        var walker = new GraphWalker(graph);

        WorkflowGraphNode currentNode;
        string? incomingInput;
        Dictionary<string, string?> outputs;

        if (execution.PendingNodeKey is not null && execution.PendingResumeHandle is not null)
        {
            // Resuming after a User Task was approved/rejected - CompleteUserTaskCommandHandler
            // already recorded the decision and re-enqueued this same execution id.
            outputs = await LoadPriorOutputsAsync(execution.Id, cancellationToken);
            var pendingNode = walker.GetByKey(execution.PendingNodeKey);
            var next = pendingNode is null ? null : walker.GetNext(pendingNode, execution.PendingResumeHandle);
            var resumeInput = outputs.TryGetValue(execution.PendingNodeKey, out var pendingOutput) ? pendingOutput : null;

            execution.ResumeFromPending();
            await _db.SaveChangesAsync(cancellationToken);

            if (next is null)
            {
                execution.MarkSucceeded();
                await _db.SaveChangesAsync(cancellationToken);
                await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                return;
            }

            currentNode = next;
            incomingInput = resumeInput;
        }
        else
        {
            execution.MarkRunning();
            await _db.SaveChangesAsync(cancellationToken);
            await _notifier.NotifyExecutionStartedAsync(execution.OrganizationId, execution.Id, cancellationToken);

            outputs = new Dictionary<string, string?>();
            currentNode = walker.GetTriggerNode();
            incomingInput = execution.InputPayloadJson;
        }

        try
        {
            while (true)
            {
                if (currentNode.Type == WorkflowNodeType.UserTask)
                {
                    await SuspendForUserTaskAsync(execution, currentNode, incomingInput, cancellationToken);
                    return;
                }

                if (currentNode.Type == WorkflowNodeType.Terminate)
                {
                    execution.MarkSucceeded();
                    await _db.SaveChangesAsync(cancellationToken);
                    await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                    return;
                }

                if (currentNode.Type == WorkflowNodeType.Loop)
                {
                    var loopResult = await ExecuteLoopAsync(execution, currentNode, incomingInput, outputs, walker, cancellationToken);
                    if (!loopResult.Success)
                    {
                        execution.MarkFailed(loopResult.ErrorMessage ?? $"Loop '{currentNode.Key}' failed.");
                        await _db.SaveChangesAsync(cancellationToken);
                        await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                        return;
                    }

                    outputs[currentNode.Key] = loopResult.AggregatedJson;
                    if (loopResult.Next is null)
                    {
                        break;
                    }

                    incomingInput = loopResult.AggregatedJson;
                    currentNode = loopResult.Next;
                    continue;
                }

                if (currentNode.Type == WorkflowNodeType.Parallel)
                {
                    var parallelResult = await ExecuteParallelAsync(execution, currentNode, incomingInput, walker, cancellationToken);
                    if (!parallelResult.Success)
                    {
                        execution.MarkFailed(parallelResult.ErrorMessage ?? $"Parallel '{currentNode.Key}' failed.");
                        await _db.SaveChangesAsync(cancellationToken);
                        await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                        return;
                    }

                    outputs[currentNode.Key] = parallelResult.CombinedJson;

                    if (parallelResult.MergeNode is null)
                    {
                        break;
                    }

                    // Execute the Merge node itself on the main (already-tracked) DbContext so it
                    // shows up as a normal, notified step - the branches already ran on their own
                    // scoped DbContexts and are done by this point.
                    var mergeStepResult = await ExecuteStepAsync(_db, _nodeExecutorRegistry, _notifier, execution, parallelResult.MergeNode, parallelResult.CombinedJson, outputs, cancellationToken);
                    if (!mergeStepResult.Success)
                    {
                        execution.MarkFailed(mergeStepResult.ErrorMessage ?? $"Node '{parallelResult.MergeNode.Key}' failed.");
                        await _db.SaveChangesAsync(cancellationToken);
                        await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                        return;
                    }

                    outputs[parallelResult.MergeNode.Key] = mergeStepResult.OutputJson;
                    var afterMerge = walker.GetNext(parallelResult.MergeNode, mergeStepResult.NextHandle);
                    if (afterMerge is null)
                    {
                        break;
                    }

                    incomingInput = mergeStepResult.OutputJson ?? parallelResult.CombinedJson;
                    currentNode = afterMerge;
                    continue;
                }

                var stepResult = await ExecuteStepAsync(_db, _nodeExecutorRegistry, _notifier, execution, currentNode, incomingInput, outputs, cancellationToken);

                if (!stepResult.Success)
                {
                    var errorNode = walker.GetNext(currentNode, "error");
                    if (errorNode is not null)
                    {
                        outputs[currentNode.Key] = null;
                        incomingInput = JsonSerializer.Serialize(new { error = stepResult.ErrorMessage, failedNodeKey = currentNode.Key });
                        currentNode = errorNode;
                        continue;
                    }

                    execution.MarkFailed(stepResult.ErrorMessage ?? $"Node '{currentNode.Key}' failed.");
                    await _db.SaveChangesAsync(cancellationToken);
                    await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
                    return;
                }

                outputs[currentNode.Key] = stepResult.OutputJson;

                var next = walker.GetNext(currentNode, stepResult.NextHandle);
                if (next is null)
                {
                    break;
                }

                incomingInput = stepResult.OutputJson ?? incomingInput;
                currentNode = next;
            }

            execution.MarkSucceeded();
            await _db.SaveChangesAsync(cancellationToken);
            await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error running execution {ExecutionId}", executionId);
            execution.MarkFailed(ex.Message);
            await _db.SaveChangesAsync(cancellationToken);
            await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
        }
    }

    private async Task SuspendForUserTaskAsync(WorkflowExecution execution, WorkflowGraphNode node, string? incomingInput, CancellationToken cancellationToken)
    {
        var stepLog = new ExecutionStepLog
        {
            WorkflowExecutionId = execution.Id,
            NodeKey = node.Key,
            NodeType = node.Type,
            Status = StepStatus.WaitingApproval,
            AttemptNumber = 1,
            StartedAt = DateTime.UtcNow,
            InputJson = incomingInput,
        };

        _db.ExecutionStepLogs.Add(stepLog);
        execution.MarkPendingApproval(node.Key, incomingInput);
        await _db.SaveChangesAsync(cancellationToken);
        await _notifier.NotifyStepUpdatedAsync(execution.OrganizationId, execution.Id, stepLog.Id, node.Key, stepLog.Status.ToString(), cancellationToken);
        await _notifier.NotifyExecutionCompletedAsync(execution.OrganizationId, execution.Id, execution.Status.ToString(), cancellationToken);
    }

    private async Task<(bool Success, string? ErrorMessage, string? AggregatedJson, WorkflowGraphNode? Next)> ExecuteLoopAsync(
        WorkflowExecution execution,
        WorkflowGraphNode loopNode,
        string? incomingInput,
        Dictionary<string, string?> outputs,
        GraphWalker walker,
        CancellationToken cancellationToken)
    {
        var arrayPath = GetConfigString(loopNode, "arrayPath");
        var items = ResolveArrayItems(incomingInput, arrayPath);

        var body = walker.GetNext(loopNode, null);
        if (body is null)
        {
            return (true, null, "[]", null);
        }

        var results = new List<string?>();
        string? lastHandle = null;

        foreach (var item in items)
        {
            var stepResult = await ExecuteStepAsync(_db, _nodeExecutorRegistry, _notifier, execution, body, item, outputs, cancellationToken);
            if (!stepResult.Success)
            {
                return (false, stepResult.ErrorMessage, null, null);
            }

            results.Add(stepResult.OutputJson);
            outputs[body.Key] = stepResult.OutputJson;
            lastHandle = stepResult.NextHandle;
        }

        var aggregated = JsonSerializer.Serialize(results.Select(r =>
            string.IsNullOrWhiteSpace(r) ? (JsonElement?)null : JsonDocument.Parse(r).RootElement));

        var next = walker.GetNext(body, lastHandle);
        return (true, null, aggregated, next);
    }

    private async Task<(bool Success, string? ErrorMessage, string? CombinedJson, WorkflowGraphNode? MergeNode)> ExecuteParallelAsync(
        WorkflowExecution execution,
        WorkflowGraphNode parallelNode,
        string? incomingInput,
        GraphWalker walker,
        CancellationToken cancellationToken)
    {
        var mergeNodeKey = GetConfigString(parallelNode, "mergeNodeKey");
        if (string.IsNullOrWhiteSpace(mergeNodeKey))
        {
            return (false, "Parallel node config is missing 'mergeNodeKey'.", null, null);
        }

        var mergeNode = walker.GetByKey(mergeNodeKey);
        if (mergeNode is null)
        {
            return (false, $"Parallel node's mergeNodeKey '{mergeNodeKey}' does not match any node in the graph.", null, null);
        }

        var branches = walker.GetAllNext(parallelNode);
        if (branches.Count == 0)
        {
            return (true, null, incomingInput, mergeNode);
        }

        var branchTasks = branches.Select(branchStart =>
            RunBranchAsync(execution.Id, branchStart, incomingInput, mergeNodeKey, walker, cancellationToken));

        var branchResults = await Task.WhenAll(branchTasks);

        var failed = branchResults.FirstOrDefault(r => !r.Success);
        if (failed.ErrorMessage is not null)
        {
            return (false, failed.ErrorMessage, null, null);
        }

        var combined = JsonSerializer.Serialize(branchResults.Select(r =>
            string.IsNullOrWhiteSpace(r.OutputJson) ? (JsonElement?)null : JsonDocument.Parse(r.OutputJson).RootElement));

        return (true, null, combined, mergeNode);
    }

    /// <summary>Walks one Parallel branch to completion on its OWN DI scope (own DbContext, own
    /// executor registry, own notifier) so multiple branches can run concurrently via
    /// Task.WhenAll without sharing a single DbContext across threads - EF Core DbContext
    /// instances are not thread-safe for concurrent use. GraphWalker is safe to share; it only
    /// reads an immutable, already-parsed WorkflowGraph.</summary>
    private async Task<(bool Success, string? ErrorMessage, string? OutputJson)> RunBranchAsync(
        Guid executionId,
        WorkflowGraphNode branchStart,
        string? branchInput,
        string mergeNodeKey,
        GraphWalker walker,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var registry = scope.ServiceProvider.GetRequiredService<INodeExecutorRegistry>();
        var notifier = scope.ServiceProvider.GetRequiredService<IExecutionNotifier>();

        var execution = await db.WorkflowExecutions.IgnoreQueryFilters().FirstAsync(e => e.Id == executionId, cancellationToken);

        var currentNode = branchStart;
        string? currentInput = branchInput;
        var localOutputs = new Dictionary<string, string?>();

        while (currentNode.Key != mergeNodeKey)
        {
            var stepResult = await ExecuteStepAsync(db, registry, notifier, execution, currentNode, currentInput, localOutputs, cancellationToken);
            if (!stepResult.Success)
            {
                return (false, stepResult.ErrorMessage, null);
            }

            localOutputs[currentNode.Key] = stepResult.OutputJson;

            var next = walker.GetNext(currentNode, stepResult.NextHandle);
            if (next is null)
            {
                // Branch ended without ever reaching the declared merge node - return its last
                // output anyway rather than failing the whole execution over a graph-authoring
                // mistake the Merge step itself doesn't strictly need to catch.
                return (true, null, stepResult.OutputJson);
            }

            currentInput = stepResult.OutputJson ?? currentInput;
            currentNode = next;
        }

        return (true, null, currentInput);
    }

    private async Task<Dictionary<string, string?>> LoadPriorOutputsAsync(Guid executionId, CancellationToken cancellationToken)
    {
        var logs = await _db.ExecutionStepLogs
            .Where(s => s.WorkflowExecutionId == executionId && s.Status == StepStatus.Succeeded)
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);

        var outputs = new Dictionary<string, string?>();
        foreach (var log in logs)
        {
            outputs[log.NodeKey] = log.OutputJson;
        }

        // The pending User Task's own step log is recorded as WaitingApproval, not Succeeded (it
        // isn't a normal success/fail outcome) - CompleteUserTaskCommandHandler updates it to
        // Succeeded with the approve/reject decision as its output once someone responds, so by
        // the time this resumes it's already covered by the query above.
        return outputs;
    }

    private static string GetConfigString(WorkflowGraphNode node, string propertyName)
    {
        var configJson = node.Config.ValueKind == JsonValueKind.Undefined ? "{}" : node.Config.GetRawText();
        using var config = JsonDocument.Parse(configJson);
        return config.RootElement.TryGetProperty(propertyName, out var el) ? el.GetString() ?? "" : "";
    }

    private static List<string> ResolveArrayItems(string? inputJson, string arrayPath)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(inputJson) ? "[]" : inputJson);
        var current = doc.RootElement;

        if (!string.IsNullOrWhiteSpace(arrayPath))
        {
            foreach (var segment in arrayPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
                {
                    return new List<string>();
                }

                current = next;
            }
        }

        if (current.ValueKind != JsonValueKind.Array)
        {
            return new List<string>();
        }

        return current.EnumerateArray().Select(e => e.GetRawText()).ToList();
    }

    private static async Task<NodeResult> ExecuteStepAsync(
        IApplicationDbContext db,
        INodeExecutorRegistry nodeExecutorRegistry,
        IExecutionNotifier notifier,
        WorkflowExecution execution,
        WorkflowGraphNode node,
        string? inputJson,
        IReadOnlyDictionary<string, string?> priorOutputs,
        CancellationToken cancellationToken)
    {
        var stepLog = new ExecutionStepLog
        {
            WorkflowExecutionId = execution.Id,
            NodeKey = node.Key,
            NodeType = node.Type,
            Status = StepStatus.Running,
            AttemptNumber = 1,
            StartedAt = DateTime.UtcNow,
            InputJson = inputJson
        };

        db.ExecutionStepLogs.Add(stepLog);
        await db.SaveChangesAsync(cancellationToken);
        await notifier.NotifyStepUpdatedAsync(execution.OrganizationId, execution.Id, stepLog.Id, node.Key, stepLog.Status.ToString(), cancellationToken);

        var executor = nodeExecutorRegistry.Resolve(node.Type);
        var context = new NodeExecutionContext
        {
            OrganizationId = execution.OrganizationId,
            NodeKey = node.Key,
            ConfigJson = node.Config.ValueKind == JsonValueKind.Undefined ? "{}" : node.Config.GetRawText(),
            InputJson = inputJson ?? "{}",
            PriorOutputs = priorOutputs
        };

        NodeResult result;
        try
        {
            result = await RetryPolicyFactory.Create().ExecuteAsync(ct => executor.ExecuteAsync(context, ct), cancellationToken);
        }
        catch (Exception ex)
        {
            result = NodeResult.Fail(ex.Message);
        }

        stepLog.Status = result.Success ? StepStatus.Succeeded : StepStatus.Failed;
        stepLog.OutputJson = result.OutputJson;
        stepLog.ErrorMessage = result.ErrorMessage;
        stepLog.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await notifier.NotifyStepUpdatedAsync(execution.OrganizationId, execution.Id, stepLog.Id, node.Key, stepLog.Status.ToString(), cancellationToken);

        return result;
    }
}
