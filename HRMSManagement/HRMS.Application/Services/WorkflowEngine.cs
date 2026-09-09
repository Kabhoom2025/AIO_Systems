using System.Text.Json;
using System.Text.Json.Serialization;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private const int MaxSteps = 100; // guards against a malformed/cyclic graph looping forever

    private readonly IWorkflowRepository _repo;
    private readonly INotificationService _notifications;

    public WorkflowEngine(IWorkflowRepository repo, INotificationService notifications)
    {
        _repo = repo;
        _notifications = notifications;
    }

    public async Task TriggerAsync(
        int orgId,
        string triggerType,
        string triggerEntityType,
        int triggerEntityId,
        Dictionary<string, object?> contextData,
        Dictionary<string, Func<Dictionary<string, string>, Task>> actionHandlers)
    {
        var published = await _repo.GetPublishedForTriggerAsync(orgId, triggerType);
        if (published == null) return; // no workflow configured — caller's default behavior stands

        var (definition, version) = published.Value;
        var path = new List<object>();

        try
        {
            var graph = JsonSerializer.Deserialize<WorkflowGraph>(version.GraphJson, JsonOptions) ?? new WorkflowGraph();
            var current = graph.Nodes.FirstOrDefault(n => n.Type == "trigger");
            if (current == null) return; // empty/malformed graph — nothing to run

            path.Add(new { node = current.Id, type = "trigger" });

            for (var step = 0; step < MaxSteps; step++)
            {
                var outgoing = graph.Edges.Where(e => e.Source == current.Id).ToList();
                WorkflowEdge? next;

                if (current.Type == "condition")
                {
                    var data = current.Data.Deserialize<ConditionNodeData>(JsonOptions) ?? new ConditionNodeData();
                    var result = EvaluateGroups(data.Groups, contextData);
                    var branch = result ? "true" : "false";
                    next = outgoing.FirstOrDefault(e => e.Branch == branch);
                    path.Add(new { node = current.Id, type = "condition", result, branch });
                }
                else
                {
                    if (current.Type == "action")
                    {
                        var data = current.Data.Deserialize<ActionNodeData>(JsonOptions) ?? new ActionNodeData();
                        await ExecuteActionAsync(orgId, data, contextData, actionHandlers);
                        path.Add(new { node = current.Id, type = "action", actionType = data.ActionType });
                    }
                    next = outgoing.FirstOrDefault();
                }

                if (next == null) break; // reached an End node or a dead end

                var target = graph.Nodes.FirstOrDefault(n => n.Id == next.Target);
                if (target == null) break;
                current = target;
            }

            _repo.AddExecution(new WorkflowExecution
            {
                OrganizationId      = orgId,
                WorkflowDefinitionId = definition.Id,
                WorkflowVersionId   = version.Id,
                TriggerEntityType   = triggerEntityType,
                TriggerEntityId     = triggerEntityId,
                Status              = "Completed",
                PathJson            = JsonSerializer.Serialize(path, JsonOptions)
            });
            await _repo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _repo.AddExecution(new WorkflowExecution
            {
                OrganizationId      = orgId,
                WorkflowDefinitionId = definition.Id,
                WorkflowVersionId   = version.Id,
                TriggerEntityType   = triggerEntityType,
                TriggerEntityId     = triggerEntityId,
                Status              = "Failed",
                ErrorMessage        = ex.Message,
                PathJson            = JsonSerializer.Serialize(path, JsonOptions)
            });
            await _repo.SaveChangesAsync();
            // Swallow — a broken workflow must never block the underlying business operation
            // (e.g. submitting a leave request) that triggered it.
        }
    }

    private async Task ExecuteActionAsync(
        int orgId,
        ActionNodeData data,
        Dictionary<string, object?> contextData,
        Dictionary<string, Func<Dictionary<string, string>, Task>> actionHandlers)
    {
        var config = data.Config ?? new Dictionary<string, string>();

        if (data.ActionType == "End") return;

        if (data.ActionType == "Notify")
        {
            var recipient = config.GetValueOrDefault("recipient", "Employee");
            var title = config.GetValueOrDefault("title", "Workflow notification");
            var message = ResolveTemplate(config.GetValueOrDefault("message", string.Empty), contextData);

            int? userId = recipient switch
            {
                "Employee" => contextData.GetValueOrDefault("EmployeeUserId") as int?,
                "Manager"  => contextData.GetValueOrDefault("ManagerUserId") as int?,
                _          => null // "HR" / anything else → org-wide broadcast
            };

            await _notifications.CreateAsync(orgId, new CreateNotificationDto
            {
                UserId  = userId,
                Title   = title,
                Message = message,
                Type    = "Info"
            });
            return;
        }

        if (actionHandlers.TryGetValue(data.ActionType, out var handler))
            await handler(config);
    }

    private static string ResolveTemplate(string template, Dictionary<string, object?> contextData)
    {
        foreach (var (key, value) in contextData)
            template = template.Replace("{" + key + "}", value?.ToString() ?? string.Empty);
        return template;
    }

    private static bool EvaluateGroups(List<ConditionGroup> groups, Dictionary<string, object?> context)
    {
        if (groups.Count == 0) return true; // an if/else with no rules configured always takes the "true" path
        return groups.Any(g => g.Conditions.Count > 0 && g.Conditions.All(c => EvaluateCondition(c, context)));
    }

    private static bool EvaluateCondition(ConditionRule rule, Dictionary<string, object?> context)
    {
        context.TryGetValue(rule.Field, out var actual);
        var actualStr = actual?.ToString() ?? string.Empty;

        return rule.Operator switch
        {
            "Equals"             => Compare(actualStr, rule.Value, (a, b) => a == b, (a, b) => a == b),
            "NotEquals"           => !Compare(actualStr, rule.Value, (a, b) => a == b, (a, b) => a == b),
            "GreaterThan"         => Compare(actualStr, rule.Value, null, (a, b) => a > b),
            "GreaterThanOrEqual"  => Compare(actualStr, rule.Value, null, (a, b) => a >= b),
            "LessThan"            => Compare(actualStr, rule.Value, null, (a, b) => a < b),
            "LessThanOrEqual"     => Compare(actualStr, rule.Value, null, (a, b) => a <= b),
            "Contains"            => actualStr.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            _                     => false
        };
    }

    private static bool Compare(string a, string b,
        Func<string, string, bool>? stringCompare,
        Func<decimal, decimal, bool> numericCompare)
    {
        if (decimal.TryParse(a, out var da) && decimal.TryParse(b, out var db))
            return numericCompare(da, db);

        if (bool.TryParse(a, out var ba) && bool.TryParse(b, out var bb))
            return stringCompare != null && ba == bb;

        return stringCompare != null && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ---------------- Graph JSON shape (kept internal — the frontend just needs to produce/consume this same shape) ----------------

    private class WorkflowGraph
    {
        public List<WorkflowNode> Nodes { get; set; } = new();
        public List<WorkflowEdge> Edges { get; set; } = new();
    }

    private class WorkflowNode
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // trigger | condition | action | end
        public JsonElement Data { get; set; }
    }

    private class WorkflowEdge
    {
        public string Id { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        /// <summary>"true" | "false" for edges leaving a condition node; null otherwise.</summary>
        public string? Branch { get; set; }
    }

    private class ConditionNodeData
    {
        public List<ConditionGroup> Groups { get; set; } = new();
    }

    private class ConditionGroup
    {
        public List<ConditionRule> Conditions { get; set; } = new();
    }

    private class ConditionRule
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private class ActionNodeData
    {
        public string ActionType { get; set; } = string.Empty;
        public Dictionary<string, string>? Config { get; set; }
    }
}
