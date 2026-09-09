using System.Text.Json;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Config shape: { "field": "a.b.c" (dot-path into InputJson), "operator":
/// "equals"|"notEquals"|"contains"|"greaterThan"|"lessThan", "value": "..." }. Returns
/// NextHandle "true" or "false" so GraphWalker follows the matching outgoing edge.</summary>
public class ConditionNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Condition;

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            using var config = JsonDocument.Parse(context.ConfigJson);
            var root = config.RootElement;

            var field = root.TryGetProperty("field", out var fieldEl) ? fieldEl.GetString() ?? "" : "";
            var op = root.TryGetProperty("operator", out var opEl) ? opEl.GetString() ?? "equals" : "equals";
            var expected = root.TryGetProperty("value", out var valueEl) ? valueEl.GetString() ?? "" : "";

            using var input = JsonDocument.Parse(string.IsNullOrWhiteSpace(context.InputJson) ? "{}" : context.InputJson);
            var actual = ConditionEvaluator.ResolvePath(input.RootElement, field);

            var matched = ConditionEvaluator.Evaluate(actual, op, expected);
            return Task.FromResult(NodeResult.Ok(context.InputJson, matched ? "true" : "false"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(NodeResult.Fail($"Condition evaluation error: {ex.Message}"));
        }
    }
}
