using System.Text.Json;

namespace FlowSphere.Execution.Nodes;

/// <summary>Shared dot-path resolution + comparison logic used by both ConditionNodeExecutor
/// (binary true/false) and DecisionNodeExecutor (N-way switch/case).</summary>
public static class ConditionEvaluator
{
    public static string? ResolvePath(JsonElement root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => current.GetRawText(),
        };
    }

    public static bool Evaluate(string? actual, string op, string expected)
    {
        return op switch
        {
            "equals" => string.Equals(actual, expected, StringComparison.Ordinal),
            "notEquals" => !string.Equals(actual, expected, StringComparison.Ordinal),
            "contains" => actual?.Contains(expected, StringComparison.OrdinalIgnoreCase) ?? false,
            "greaterThan" => TryCompareNumeric(actual, expected, out var cmp) && cmp > 0,
            "lessThan" => TryCompareNumeric(actual, expected, out var cmp2) && cmp2 < 0,
            _ => false,
        };
    }

    private static bool TryCompareNumeric(string? actual, string expected, out int comparison)
    {
        comparison = 0;
        if (!double.TryParse(actual, out var a) || !double.TryParse(expected, out var b))
        {
            return false;
        }

        comparison = a.CompareTo(b);
        return true;
    }
}
