using System.Text.Json;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>N-way switch/case routing. Config shape: { "field": "a.b.c", "operator": "equals"
/// (same operator set as Condition), "cases": [{ "value": "...", "handle": "case1" }, ...],
/// "defaultHandle": "default" }. Evaluates the field against each case in order using the same
/// operator for all cases, returns the first match's handle, or defaultHandle if none match -
/// GraphWalker follows whichever outgoing edge's SourceHandle equals that value.</summary>
public class DecisionNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Decision;

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            using var config = JsonDocument.Parse(context.ConfigJson);
            var root = config.RootElement;

            var field = root.TryGetProperty("field", out var fieldEl) ? fieldEl.GetString() ?? "" : "";
            var op = root.TryGetProperty("operator", out var opEl) ? opEl.GetString() ?? "equals" : "equals";
            var defaultHandle = root.TryGetProperty("defaultHandle", out var defEl) ? defEl.GetString() ?? "default" : "default";

            using var input = JsonDocument.Parse(string.IsNullOrWhiteSpace(context.InputJson) ? "{}" : context.InputJson);
            var actual = ConditionEvaluator.ResolvePath(input.RootElement, field);

            if (root.TryGetProperty("cases", out var casesEl) && casesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var caseEl in casesEl.EnumerateArray())
                {
                    var value = caseEl.TryGetProperty("value", out var valueEl) ? valueEl.GetString() ?? "" : "";
                    var handle = caseEl.TryGetProperty("handle", out var handleEl) ? handleEl.GetString() : null;

                    if (!string.IsNullOrEmpty(handle) && ConditionEvaluator.Evaluate(actual, op, value))
                    {
                        return Task.FromResult(NodeResult.Ok(context.InputJson, handle));
                    }
                }
            }

            return Task.FromResult(NodeResult.Ok(context.InputJson, defaultHandle));
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.Fail($"Decision evaluation error: {ex.Message}"));
        }
    }
}
