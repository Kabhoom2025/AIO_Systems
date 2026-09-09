using System.Text.Json;
using System.Text.Json.Serialization;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Engine;

public class WorkflowGraphNode
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public WorkflowNodeType Type { get; set; }

    [JsonPropertyName("config")]
    public JsonElement Config { get; set; }
}

public class WorkflowGraphEdge
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    /// <summary>Only meaningful out of Condition nodes: "true" or "false".</summary>
    [JsonPropertyName("sourceHandle")]
    public string? SourceHandle { get; set; }
}

public class WorkflowGraph
{
    [JsonPropertyName("nodes")]
    public List<WorkflowGraphNode> Nodes { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<WorkflowGraphEdge> Edges { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static WorkflowGraph Parse(string graphJson)
    {
        return JsonSerializer.Deserialize<WorkflowGraph>(graphJson, Options)
            ?? new WorkflowGraph();
    }
}
