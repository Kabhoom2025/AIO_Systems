using System.Text.Json;
using System.Linq;

namespace FlowSphere.Application.Common;

/// <summary>Small helpers for pulling a single config value out of a WorkflowGraph-shaped JSON
/// string (see client's WorkflowGraph type / WorkflowGraphGenerator's schema) by node key,
/// without needing to fully deserialize the graph into domain types.</summary>
public static class WorkflowGraphNodeConfig
{
    /// <summary>Reads nodes[].config.assigneeLabel for the node matching nodeKey - used to show
    /// who a pending UserTask (accept/reject) step is expected to be reviewed by.</summary>
    public static string? GetAssigneeLabel(string graphJson, string? nodeKey)
    {
        if (string.IsNullOrEmpty(nodeKey))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(graphJson);
            if (!doc.RootElement.TryGetProperty("nodes", out var nodesEl) || nodesEl.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var node in nodesEl.EnumerateArray())
            {
                var key = node.TryGetProperty("key", out var keyEl) && keyEl.ValueKind == JsonValueKind.String ? keyEl.GetString() : null;
                if (key != nodeKey)
                {
                    continue;
                }

                if (node.TryGetProperty("config", out var configEl)
                    && configEl.TryGetProperty("assigneeLabel", out var labelEl)
                    && labelEl.ValueKind == JsonValueKind.String)
                {
                    var label = labelEl.GetString();
                    return string.IsNullOrWhiteSpace(label) ? null : label;
                }

                return null;
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Reads nodes[].config.roles for the node matching nodeKey - an empty/missing list
    /// means unrestricted (any user with workflows.approve may resolve the step), preserving
    /// today's behavior for every workflow authored before this field existed.</summary>
    public static string[] GetRoles(string graphJson, string? nodeKey)
    {
        if (string.IsNullOrEmpty(nodeKey))
        {
            return Array.Empty<string>();
        }

        try
        {
            using var doc = JsonDocument.Parse(graphJson);
            if (!doc.RootElement.TryGetProperty("nodes", out var nodesEl) || nodesEl.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<string>();
            }

            foreach (var node in nodesEl.EnumerateArray())
            {
                var key = node.TryGetProperty("key", out var keyEl) && keyEl.ValueKind == JsonValueKind.String ? keyEl.GetString() : null;
                if (key != nodeKey)
                {
                    continue;
                }

                if (node.TryGetProperty("config", out var configEl)
                    && configEl.TryGetProperty("roles", out var rolesEl)
                    && rolesEl.ValueKind == JsonValueKind.Array)
                {
                    return rolesEl.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .ToArray();
                }

                return Array.Empty<string>();
            }

            return Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>Raw passthrough of nodes[].config for the node matching nodeKey - lets the
    /// approval UI render Task Message/Comments-enabled/Save-as-draft-enabled/System-default-
    /// button config without the server needing a typed model for every new Step property (same
    /// opaque-JSON convention FormSchemaJson already uses).</summary>
    public static string? GetNodeConfigJson(string graphJson, string? nodeKey)
    {
        if (string.IsNullOrEmpty(nodeKey))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(graphJson);
            if (!doc.RootElement.TryGetProperty("nodes", out var nodesEl) || nodesEl.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var node in nodesEl.EnumerateArray())
            {
                var key = node.TryGetProperty("key", out var keyEl) && keyEl.ValueKind == JsonValueKind.String ? keyEl.GetString() : null;
                if (key != nodeKey)
                {
                    continue;
                }

                return node.TryGetProperty("config", out var configEl) ? configEl.GetRawText() : null;
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Raw passthrough array of every outgoing edge's own data (Action button
    /// name/color/icon/confirmation/notifications/toaster message) for the node matching
    /// nodeKey - each entry also carries the edge's sourceHandle and target node key so the
    /// client can match a rendered Action button back to which one was clicked.</summary>
    public static string GetOutgoingActionsJson(string graphJson, string? nodeKey)
    {
        if (string.IsNullOrEmpty(nodeKey))
        {
            return "[]";
        }

        try
        {
            using var doc = JsonDocument.Parse(graphJson);
            if (!doc.RootElement.TryGetProperty("edges", out var edgesEl) || edgesEl.ValueKind != JsonValueKind.Array)
            {
                return "[]";
            }

            var actions = new List<object>();
            foreach (var edge in edgesEl.EnumerateArray())
            {
                var source = edge.TryGetProperty("source", out var sourceEl) && sourceEl.ValueKind == JsonValueKind.String ? sourceEl.GetString() : null;
                if (source != nodeKey)
                {
                    continue;
                }

                var target = edge.TryGetProperty("target", out var targetEl) && targetEl.ValueKind == JsonValueKind.String ? targetEl.GetString() : null;
                var sourceHandle = edge.TryGetProperty("sourceHandle", out var handleEl) && handleEl.ValueKind == JsonValueKind.String ? handleEl.GetString() : null;
                var data = edge.TryGetProperty("data", out var dataEl) ? dataEl : (JsonElement?)null;

                actions.Add(new { target, sourceHandle, data });
            }

            return JsonSerializer.Serialize(actions);
        }
        catch (JsonException)
        {
            return "[]";
        }
    }
}
