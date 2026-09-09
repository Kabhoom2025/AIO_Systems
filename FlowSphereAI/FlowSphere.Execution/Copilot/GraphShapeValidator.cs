using System.Text.Json;

namespace FlowSphere.Execution.Copilot;

/// <summary>Structural validation for an AI-generated workflow graph, beyond what the designer's
/// own save path checks - control-flow node types (Decision/Loop/Parallel/UserTask) have shape
/// requirements (a mergeNodeKey that actually exists and is a Merge node, edges for every
/// Decision case, etc.) that are easy for an LLM to get subtly wrong, so this catches those
/// before the graph ever reaches the canvas instead of failing confusingly at execution time.</summary>
public static class GraphShapeValidator
{
    public static bool TryValidate(string graphJson, out string? error)
    {
        error = null;

        try
        {
            using var doc = JsonDocument.Parse(graphJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("nodes", out var nodesEl) || nodesEl.ValueKind != JsonValueKind.Array || nodesEl.GetArrayLength() == 0)
            {
                error = "The graph has no nodes.";
                return false;
            }

            if (!root.TryGetProperty("edges", out var edgesEl) || edgesEl.ValueKind != JsonValueKind.Array)
            {
                error = "The graph has no edges array.";
                return false;
            }

            var nodeTypeByKey = new Dictionary<string, string>();
            var nodeConfigByKey = new Dictionary<string, JsonElement>();

            foreach (var node in nodesEl.EnumerateArray())
            {
                var key = node.TryGetProperty("key", out var keyEl) && keyEl.ValueKind == JsonValueKind.String ? keyEl.GetString() : null;
                var type = node.TryGetProperty("type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String ? typeEl.GetString() : null;

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(type))
                {
                    error = "Every node needs a non-empty 'key' and 'type'.";
                    return false;
                }

                if (!nodeTypeByKey.TryAdd(key, type))
                {
                    error = $"Duplicate node key '{key}'.";
                    return false;
                }

                nodeConfigByKey[key] = node.TryGetProperty("config", out var configEl) ? configEl : default;
            }

            var triggerKeys = nodeTypeByKey.Where(kv => kv.Value == "Trigger").Select(kv => kv.Key).ToList();
            if (triggerKeys.Count != 1)
            {
                error = $"The graph must have exactly one Trigger node (found {triggerKeys.Count}).";
                return false;
            }

            var outgoingByNode = new Dictionary<string, List<(string Target, string? Handle)>>();

            foreach (var edge in edgesEl.EnumerateArray())
            {
                var source = edge.TryGetProperty("source", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
                var target = edge.TryGetProperty("target", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                var handle = edge.TryGetProperty("sourceHandle", out var h) && h.ValueKind == JsonValueKind.String ? h.GetString() : null;

                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                {
                    error = "Every edge needs a non-empty 'source' and 'target'.";
                    return false;
                }

                if (!nodeTypeByKey.ContainsKey(source))
                {
                    error = $"An edge references unknown source node '{source}'.";
                    return false;
                }

                if (!nodeTypeByKey.ContainsKey(target))
                {
                    error = $"An edge references unknown target node '{target}'.";
                    return false;
                }

                if (!outgoingByNode.TryGetValue(source, out var list))
                {
                    list = new List<(string Target, string? Handle)>();
                    outgoingByNode[source] = list;
                }

                list.Add((target, handle));
            }

            var triggerKey = triggerKeys[0];
            var triggerHasIncoming = edgesEl.EnumerateArray()
                .Any(e => e.TryGetProperty("target", out var t) && t.ValueKind == JsonValueKind.String && t.GetString() == triggerKey);
            if (triggerHasIncoming)
            {
                error = "The Trigger node must not have any incoming edges.";
                return false;
            }

            foreach (var (key, type) in nodeTypeByKey)
            {
                var outgoing = outgoingByNode.TryGetValue(key, out var list) ? list : new List<(string Target, string? Handle)>();
                var config = nodeConfigByKey[key];

                switch (type)
                {
                    case "Decision":
                        if (!ValidateDecision(key, config, outgoing, out error))
                        {
                            return false;
                        }

                        break;

                    case "Loop":
                        if (outgoing.Count != 1)
                        {
                            error = $"Loop node '{key}' must have exactly one outgoing edge (its body), found {outgoing.Count}.";
                            return false;
                        }

                        break;

                    case "Parallel":
                        if (!ValidateParallel(key, config, outgoing, nodeTypeByKey, out error))
                        {
                            return false;
                        }

                        break;

                    case "UserTask":
                        var handles = outgoing.Select(o => o.Handle).ToHashSet();
                        if (!handles.Contains("approved") || !handles.Contains("rejected"))
                        {
                            error = $"UserTask node '{key}' must have outgoing edges for both the 'approved' and 'rejected' handles.";
                            return false;
                        }

                        break;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            error = "The AI's response was not valid JSON.";
            return false;
        }
    }

    private static bool ValidateDecision(string key, JsonElement config, List<(string Target, string? Handle)> outgoing, out string? error)
    {
        error = null;
        var expectedHandles = new List<string>();

        if (config.ValueKind == JsonValueKind.Object)
        {
            if (config.TryGetProperty("cases", out var casesEl) && casesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in casesEl.EnumerateArray())
                {
                    if (c.TryGetProperty("handle", out var hEl) && hEl.ValueKind == JsonValueKind.String)
                    {
                        expectedHandles.Add(hEl.GetString()!);
                    }
                }
            }

            if (config.TryGetProperty("defaultHandle", out var defEl) && defEl.ValueKind == JsonValueKind.String)
            {
                expectedHandles.Add(defEl.GetString()!);
            }
        }

        var actualHandles = outgoing.Select(o => o.Handle).Where(h => h is not null).ToHashSet();
        foreach (var expected in expectedHandles)
        {
            if (!actualHandles.Contains(expected))
            {
                error = $"Decision node '{key}' has no outgoing edge for handle '{expected}'.";
                return false;
            }
        }

        return true;
    }

    private static bool ValidateParallel(
        string key,
        JsonElement config,
        List<(string Target, string? Handle)> outgoing,
        Dictionary<string, string> nodeTypeByKey,
        out string? error)
    {
        error = null;

        var mergeNodeKey = config.ValueKind == JsonValueKind.Object
            && config.TryGetProperty("mergeNodeKey", out var mkEl)
            && mkEl.ValueKind == JsonValueKind.String
                ? mkEl.GetString()
                : null;

        if (string.IsNullOrEmpty(mergeNodeKey))
        {
            error = $"Parallel node '{key}' config is missing 'mergeNodeKey'.";
            return false;
        }

        if (!nodeTypeByKey.TryGetValue(mergeNodeKey, out var mergeType))
        {
            error = $"Parallel node '{key}' references mergeNodeKey '{mergeNodeKey}', which doesn't exist in the graph.";
            return false;
        }

        if (mergeType != "Merge")
        {
            error = $"Parallel node '{key}'s mergeNodeKey '{mergeNodeKey}' must refer to a Merge node (found '{mergeType}').";
            return false;
        }

        if (outgoing.Count < 2)
        {
            error = $"Parallel node '{key}' must have at least two outgoing branches, found {outgoing.Count}.";
            return false;
        }

        return true;
    }
}
