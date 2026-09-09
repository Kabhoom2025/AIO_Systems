using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Automation;

public record WorkflowLogEntry(string Action, string Result, DateTime Timestamp);

/// <summary>Cross-cutting service invoked from existing command handlers on WorkItem/Sprint
/// lifecycle events — same wiring style as Phase 4's INotificationDispatcher (see
/// NotificationDispatcher.cs). For each enabled WorkflowDefinition matching the trigger+project (or
/// org-wide if ProjectId is null), evaluates all WorkflowConditions (ANDed) and, if they all pass,
/// creates a WorkflowRun and executes WorkflowActions in Order.</summary>
public interface IWorkflowEngine
{
    Task EvaluateTriggerAsync(WorkflowTriggerType triggerType, Guid projectId, WorkItem? triggerEntity, CancellationToken cancellationToken = default);

    /// <summary>Scheduled workflows have no triggering WorkItem — they skip condition evaluation and
    /// execute their configured actions unconditionally. Called by the Hangfire recurring job.</summary>
    Task ExecuteScheduledWorkflowAsync(Guid workflowDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>Called from POST /automation/approvals/{id}/decide. Approved resumes the paused
    /// WorkflowRun and executes its remaining actions; Rejected marks the run Failed and stops.</summary>
    Task ResumeRunAsync(Guid workflowRunId, bool approved, CancellationToken cancellationToken = default);
}

public class WorkflowEngine : IWorkflowEngine
{
    private readonly IProjectFlowDbContext _db;
    private readonly INotificationDispatcher _notifications;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(IProjectFlowDbContext db, INotificationDispatcher notifications,
        IHttpClientFactory httpClientFactory, IDateTimeProvider clock, ILogger<WorkflowEngine> logger)
    {
        _db = db; _notifications = notifications; _httpClientFactory = httpClientFactory; _clock = clock; _logger = logger;
    }

    public async Task EvaluateTriggerAsync(WorkflowTriggerType triggerType, Guid projectId, WorkItem? triggerEntity, CancellationToken cancellationToken = default)
    {
        // Guard against infinite trigger loops: workflow actions (ChangeStatus/AssignUser/AddLabel)
        // mutate the WorkItem directly via the DbContext below — they never call back into
        // EvaluateTriggerAsync — so a workflow that changes status can never re-fire
        // WorkItemStatusChanged and recurse. Recursion depth is therefore structurally capped at 0.
        var workflows = await _db.WorkflowDefinitions
            .Include(w => w.Conditions)
            .Include(w => w.Actions)
            .Where(w => w.IsEnabled && w.TriggerType == triggerType && (w.ProjectId == null || w.ProjectId == projectId))
            .ToListAsync(cancellationToken);

        foreach (var workflow in workflows)
        {
            if (triggerEntity != null && !EvaluateConditions(workflow.Conditions, triggerEntity))
                continue;

            await RunWorkflowAsync(workflow, triggerEntity?.Id, cancellationToken);
        }
    }

    public async Task ExecuteScheduledWorkflowAsync(Guid workflowDefinitionId, CancellationToken cancellationToken = default)
    {
        var workflow = await _db.WorkflowDefinitions.Include(w => w.Actions)
            .FirstOrDefaultAsync(w => w.Id == workflowDefinitionId, cancellationToken);
        if (workflow == null || !workflow.IsEnabled) return;

        await RunWorkflowAsync(workflow, null, cancellationToken);
    }

    public async Task ResumeRunAsync(Guid workflowRunId, bool approved, CancellationToken cancellationToken = default)
    {
        var run = await _db.WorkflowRuns.FirstOrDefaultAsync(r => r.Id == workflowRunId, cancellationToken)
            ?? throw new NotFoundException("WorkflowRun", workflowRunId);
        if (run.Status != WorkflowRunStatus.AwaitingApproval) return;

        var workflow = await _db.WorkflowDefinitions.Include(w => w.Actions)
            .FirstOrDefaultAsync(w => w.Id == run.WorkflowDefinitionId, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", run.WorkflowDefinitionId);

        var log = ParseLog(run.LogJson);

        if (!approved)
        {
            log.Add(new WorkflowLogEntry("Approval", "Rejected — run stopped.", _clock.UtcNow));
            run.Status = WorkflowRunStatus.Failed;
            run.CompletedAt = _clock.UtcNow;
            run.LogJson = JsonSerializer.Serialize(log);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        // Resume index: each action (including the RequireApproval action that paused this run)
        // appends exactly one log entry, so the log's current length is precisely how many actions
        // have already executed.
        var actions = workflow.Actions.OrderBy(a => a.Order).ToList();
        run.Status = WorkflowRunStatus.Running;
        await ExecuteActionsFromAsync(run, actions, log.Count, cancellationToken);
    }

    private async Task RunWorkflowAsync(WorkflowDefinition workflow, Guid? triggerEntityId, CancellationToken cancellationToken)
    {
        var run = new WorkflowRun
        {
            WorkflowDefinitionId = workflow.Id,
            TriggerEntityId = triggerEntityId,
            Status = WorkflowRunStatus.Running,
            LogJson = "[]"
        };
        _db.WorkflowRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var actions = workflow.Actions.OrderBy(a => a.Order).ToList();
        await ExecuteActionsFromAsync(run, actions, 0, cancellationToken);
    }

    private async Task ExecuteActionsFromAsync(WorkflowRun run, List<WorkflowAction> actions, int startIndex, CancellationToken cancellationToken)
    {
        var log = ParseLog(run.LogJson);

        for (var i = startIndex; i < actions.Count; i++)
        {
            var action = actions[i];
            try
            {
                var result = await ExecuteActionAsync(action, run, cancellationToken);
                log.Add(new WorkflowLogEntry(action.ActionType.ToString(), result, _clock.UtcNow));

                if (action.ActionType == WorkflowActionType.RequireApproval)
                {
                    run.Status = WorkflowRunStatus.AwaitingApproval;
                    run.LogJson = JsonSerializer.Serialize(log);
                    await _db.SaveChangesAsync(cancellationToken);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WorkflowEngine: action {ActionType} failed for run {RunId}", action.ActionType, run.Id);
                log.Add(new WorkflowLogEntry(action.ActionType.ToString(), $"Error: {ex.Message}", _clock.UtcNow));
                run.Status = WorkflowRunStatus.Failed;
                run.CompletedAt = _clock.UtcNow;
                run.LogJson = JsonSerializer.Serialize(log);
                await _db.SaveChangesAsync(cancellationToken);
                return;
            }
        }

        run.Status = WorkflowRunStatus.Succeeded;
        run.CompletedAt = _clock.UtcNow;
        run.LogJson = JsonSerializer.Serialize(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ExecuteActionAsync(WorkflowAction action, WorkflowRun run, CancellationToken cancellationToken)
    {
        var triggerEntityId = run.TriggerEntityId;
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(action.ActionConfigJson) ? "{}" : action.ActionConfigJson);
        var root = doc.RootElement;

        switch (action.ActionType)
        {
            case WorkflowActionType.ChangeStatus:
            {
                if (triggerEntityId == null) return "Skipped — no triggering WorkItem.";
                var statusStr = root.TryGetProperty("status", out var s) ? s.GetString() : null;
                if (statusStr == null || !Enum.TryParse<WorkItemStatus>(statusStr, ignoreCase: true, out var newStatus))
                    return $"Error: invalid or missing 'status' in action config ('{statusStr}').";

                var item = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == triggerEntityId.Value, cancellationToken);
                if (item == null) return "Error: WorkItem not found.";
                item.Status = newStatus;
                item.UpdatedAt = _clock.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                return $"Changed status to {newStatus}.";
            }
            case WorkflowActionType.AssignUser:
            {
                if (triggerEntityId == null) return "Skipped — no triggering WorkItem.";
                var userIdStr = root.TryGetProperty("userId", out var u) ? u.GetString() : null;
                if (!Guid.TryParse(userIdStr, out var userId)) return $"Error: invalid or missing 'userId' in action config.";

                var item = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == triggerEntityId.Value, cancellationToken);
                if (item == null) return "Error: WorkItem not found.";
                item.AssigneeUserId = userId;
                item.UpdatedAt = _clock.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                return $"Assigned to user {userId}.";
            }
            case WorkflowActionType.AddLabel:
            {
                if (triggerEntityId == null) return "Skipped — no triggering WorkItem.";
                var labelIdStr = root.TryGetProperty("labelId", out var l) ? l.GetString() : null;
                if (!Guid.TryParse(labelIdStr, out var labelId)) return "Error: invalid or missing 'labelId' in action config.";

                var exists = await _db.WorkItemLabels.AnyAsync(wl => wl.WorkItemId == triggerEntityId.Value && wl.LabelId == labelId, cancellationToken);
                if (!exists)
                {
                    _db.WorkItemLabels.Add(new WorkItemLabel { WorkItemId = triggerEntityId.Value, LabelId = labelId });
                    await _db.SaveChangesAsync(cancellationToken);
                }
                return $"Label {labelId} added.";
            }
            case WorkflowActionType.SendNotification:
            {
                Guid? targetUserId = root.TryGetProperty("userId", out var uidEl) && Guid.TryParse(uidEl.GetString(), out var uid) ? uid : null;
                if (targetUserId == null && triggerEntityId != null)
                {
                    var item = await _db.WorkItems.AsNoTracking().FirstOrDefaultAsync(w => w.Id == triggerEntityId.Value, cancellationToken);
                    targetUserId = item?.AssigneeUserId ?? item?.ReporterUserId;
                }
                if (targetUserId == null) return "Skipped — no target user resolved for notification.";

                var title = root.TryGetProperty("title", out var t) ? t.GetString() ?? "Workflow automation" : "Workflow automation";
                var body = root.TryGetProperty("body", out var b) ? b.GetString() ?? "A workflow action fired." : "A workflow action fired.";
                await _notifications.DispatchAsync(targetUserId.Value, NotificationType.WorkflowAutomation, title, body, null, cancellationToken);
                return $"Notified user {targetUserId}.";
            }
            case WorkflowActionType.CallWebhook:
            {
                var url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;
                if (string.IsNullOrWhiteSpace(url)) return "Error: missing 'url' in action config.";
                var method = root.TryGetProperty("method", out var m) ? (m.GetString() ?? "POST") : "POST";

                object payload = new { };
                if (triggerEntityId != null)
                {
                    var item = await _db.WorkItems.AsNoTracking().FirstOrDefaultAsync(w => w.Id == triggerEntityId.Value, cancellationToken);
                    if (item != null)
                        payload = new { item.Id, item.Title, Status = item.Status.ToString(), Priority = item.Priority.ToString(), item.ProjectId, item.AssigneeUserId };
                }

                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(new HttpMethod(method), url) { Content = JsonContent.Create(payload) };
                var response = await client.SendAsync(request, cancellationToken);
                return $"Webhook {method} {url} returned {(int)response.StatusCode}.";
            }
            case WorkflowActionType.RequireApproval:
            {
                var approverIdStr = root.TryGetProperty("approverUserId", out var a) ? a.GetString() : null;
                if (!Guid.TryParse(approverIdStr, out var approverId)) return "Error: invalid or missing 'approverUserId' in action config.";

                _db.WorkflowApprovalRequests.Add(new WorkflowApprovalRequest { WorkflowRunId = run.Id, RequestedApproverUserId = approverId });
                await _db.SaveChangesAsync(cancellationToken);
                return $"Approval requested from user {approverId}.";
            }
            default:
                return "Unknown action type — skipped.";
        }
    }

    /// <summary>All conditions on a WorkflowDefinition are ANDed together — no nested groups, kept
    /// simple per spec. Public + static so it's directly unit-testable without standing up the full
    /// engine (see WorkflowEngineTests).</summary>
    public static bool EvaluateConditions(IEnumerable<WorkflowCondition> conditions, WorkItem item)
        => conditions.All(c => EvaluateCondition(c, item));

    public static bool EvaluateCondition(WorkflowCondition condition, WorkItem item)
    {
        // Small reflection-free switch on FieldPath, per spec.
        string? actual = condition.FieldPath switch
        {
            "Priority" => item.Priority.ToString(),
            "Status" => item.Status.ToString(),
            "Type" => item.Type.ToString(),
            "AssigneeUserId" => item.AssigneeUserId?.ToString(),
            "StoryPoints" => item.StoryPoints?.ToString(),
            _ => null
        };

        return condition.Operator switch
        {
            WorkflowConditionOperator.Equals => string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase),
            WorkflowConditionOperator.NotEquals => !string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase),
            WorkflowConditionOperator.GreaterThan => CompareNumeric(actual, condition.Value) > 0,
            WorkflowConditionOperator.LessThan => CompareNumeric(actual, condition.Value) < 0,
            WorkflowConditionOperator.Contains => actual != null && actual.Contains(condition.Value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static int CompareNumeric(string? actual, string expected)
    {
        if (double.TryParse(actual, out var a) && double.TryParse(expected, out var e))
            return a.CompareTo(e);
        return string.Compare(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static List<WorkflowLogEntry> ParseLog(string logJson)
    {
        if (string.IsNullOrWhiteSpace(logJson)) return new List<WorkflowLogEntry>();
        return JsonSerializer.Deserialize<List<WorkflowLogEntry>>(logJson) ?? new List<WorkflowLogEntry>();
    }
}
