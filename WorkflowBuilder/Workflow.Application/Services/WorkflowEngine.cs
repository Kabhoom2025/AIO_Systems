using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Workflow.Application.Interfaces;
using Workflow.Domain.Entities;

namespace Workflow.Application.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private const int MaxSteps = 100; // guards against a malformed/cyclic graph looping forever

    private readonly IWorkflowRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public WorkflowEngine(IWorkflowRepository repo, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WorkflowExecution> RunAsync(
        int orgId,
        WorkflowDefinition definition,
        WorkflowVersion version,
        string triggerEntityType,
        Dictionary<string, object?> contextData,
        Func<WorkflowStepEvent, Task>? onStep = null)
    {
        var path = new List<WorkflowStepEvent>();

        // Checkpoint immediately: a crash mid-run leaves this row visible as "Running" with
        // whatever steps had completed, instead of no record at all. It will not auto-resume.
        var execution = new WorkflowExecution
        {
            OrganizationId       = orgId,
            WorkflowDefinitionId = definition.Id,
            WorkflowVersionId    = version.Id,
            TriggerEntityType    = triggerEntityType,
            TriggerEntityId      = 0,
            Status               = "Running",
            PathJson             = JsonSerializer.Serialize(path, JsonOptions)
        };
        _repo.AddExecution(execution);
        await _repo.SaveChangesAsync();

        async Task RecordStepAsync(WorkflowStepEvent evt)
        {
            path.Add(evt);
            if (onStep != null) await onStep(evt);
            execution.PathJson = JsonSerializer.Serialize(path, JsonOptions);
            await _repo.SaveChangesAsync();
        }

        try
        {
            var graph = JsonSerializer.Deserialize<WorkflowGraph>(version.GraphJson, JsonOptions) ?? new WorkflowGraph();
            var current = graph.Nodes.FirstOrDefault(n => n.Type == "trigger");
            if (current == null)
                throw new InvalidOperationException("This workflow's graph has no trigger node.");

            await RecordStepAsync(new WorkflowStepEvent { Node = current.Id, Type = "trigger" });

            for (var step = 0; step < MaxSteps; step++)
            {
                var outgoing = graph.Edges.Where(e => e.Source == current.Id).ToList();
                WorkflowEdge? next;

                if (current.Type == "condition")
                {
                    var data = current.Data.Deserialize<ConditionNodeData>(JsonOptions) ?? new ConditionNodeData();
                    var matched = data.Branches.FirstOrDefault(b => b.Conditions.All(c => EvaluateCondition(c, contextData)));
                    next = matched != null
                        ? outgoing.FirstOrDefault(e => e.Branch == matched.Id)
                        : outgoing.FirstOrDefault(e => e.Branch == null);
                    await RecordStepAsync(new WorkflowStepEvent
                    {
                        Node = current.Id, Type = "condition",
                        MatchedBranch = matched?.Id, MatchedBranchName = matched?.Name
                    });
                }
                else
                {
                    if (current.Type == "action")
                    {
                        var data = current.Data.Deserialize<ActionNodeData>(JsonOptions) ?? new ActionNodeData();
                        var detail = await ExecuteActionAsync(data, contextData);
                        await RecordStepAsync(new WorkflowStepEvent
                        {
                            Node = current.Id, Type = "action",
                            ActionType = data.ActionType, Result = detail
                        });
                    }
                    next = outgoing.FirstOrDefault();
                }

                if (next == null) break; // reached an End node or a dead end

                var target = graph.Nodes.FirstOrDefault(n => n.Id == next.Target);
                if (target == null) break;
                current = target;
            }

            execution.Status = "Completed";
            await _repo.SaveChangesAsync();
            if (onStep != null) await onStep(new WorkflowStepEvent { Type = "done", Result = "Completed" });
            return execution;
        }
        catch (Exception ex)
        {
            execution.Status       = "Failed";
            execution.ErrorMessage = ex.Message;
            await _repo.SaveChangesAsync();
            if (onStep != null) await onStep(new WorkflowStepEvent { Type = "done", Result = "Failed" });
            return execution;
        }
    }

    private async Task<string> ExecuteActionAsync(ActionNodeData data, Dictionary<string, object?> contextData)
    {
        var config = data.Config ?? new Dictionary<string, string>();

        switch (data.ActionType)
        {
            case "End":
                return "Reached end of workflow.";

            case "Delay":
            {
                var requested = decimal.TryParse(config.GetValueOrDefault("seconds", "0"), out var s) ? s : 0;
                var clamped = Math.Clamp(requested, 0, 30);
                await Task.Delay(TimeSpan.FromSeconds((double)clamped));
                return $"Delayed {clamped}s.";
            }

            case "Notify":
            {
                var title = ResolveTemplate(config.GetValueOrDefault("title", string.Empty), contextData);
                var message = ResolveTemplate(config.GetValueOrDefault("message", string.Empty), contextData);
                return $"Notify: {title} — {message}";
            }

            case "SetVariable":
            {
                var key = config.GetValueOrDefault("key", string.Empty);
                var value = ResolveTemplate(config.GetValueOrDefault("value", string.Empty), contextData);
                if (!string.IsNullOrWhiteSpace(key))
                    contextData[key] = value;
                return $"{key} = {value}";
            }

            case "LogMessage":
            {
                var message = ResolveTemplate(config.GetValueOrDefault("message", string.Empty), contextData);
                return message;
            }

            case "HttpRequest":
            {
                var url = ResolveTemplate(config.GetValueOrDefault("url", string.Empty), contextData);
                var method = config.GetValueOrDefault("method", "GET").ToUpperInvariant();
                var body = ResolveTemplate(config.GetValueOrDefault("body", string.Empty), contextData);

                if (string.IsNullOrWhiteSpace(url))
                    return "Skipped — no URL configured.";

                var client = _httpClientFactory.CreateClient("workflow-actions");
                using var request = new HttpRequestMessage(new HttpMethod(method), url);
                if (!string.IsNullOrWhiteSpace(body) && method is "POST" or "PUT" or "PATCH")
                    request.Content = JsonContent.Create(body);

                using var response = await client.SendAsync(request);
                return $"{method} {url} -> {(int)response.StatusCode} {response.StatusCode}";
            }

            default:
                return $"Unknown action type '{data.ActionType}' — skipped.";
        }
    }

    private static string ResolveTemplate(string template, Dictionary<string, object?> contextData)
    {
        foreach (var (key, value) in contextData)
            template = template.Replace("{" + key + "}", value?.ToString() ?? string.Empty);
        return template;
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
        public List<DecisionBranch> Branches { get; set; } = new();
    }

    private class DecisionBranch
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
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

/// <summary>One step recorded during a run — persisted (as the execution's PathJson array) and,
/// when a run is streamed, emitted to the caller as it happens. Type "done" is a synthetic final
/// event (not part of PathJson) signalling the run finished with Result "Completed"/"Failed".</summary>
public class WorkflowStepEvent
{
    public string? Node { get; set; }
    public string? Type { get; set; } // trigger | condition | action | done
    public string? ActionType { get; set; }
    public string? Result { get; set; }
    public string? MatchedBranch { get; set; }
    public string? MatchedBranchName { get; set; }
}
