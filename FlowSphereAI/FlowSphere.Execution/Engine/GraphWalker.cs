using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Engine;

/// <summary>Walks a WorkflowGraph starting from its single Trigger node, following edges.
/// Condition nodes branch via the outgoing edge whose SourceHandle matches the node's result
/// ("true"/"false").</summary>
public class GraphWalker
{
    private readonly WorkflowGraph _graph;

    public GraphWalker(WorkflowGraph graph)
    {
        _graph = graph;
    }

    public WorkflowGraphNode GetTriggerNode()
    {
        var trigger = _graph.Nodes.FirstOrDefault(n => n.Type == WorkflowNodeType.Trigger);
        if (trigger is null)
        {
            throw new DomainException("Workflow graph has no Trigger node.");
        }

        return trigger;
    }

    /// <summary>Returns the next node to execute after `current`, following the edge that
    /// matches `handle` (null for non-branching nodes; "true"/"false" for Condition). Returns
    /// null when there is no outgoing edge (end of the workflow).</summary>
    public WorkflowGraphNode? GetNext(WorkflowGraphNode current, string? handle)
    {
        var candidateEdges = _graph.Edges.Where(e => e.Source == current.Key);

        var edge = handle is null
            ? candidateEdges.FirstOrDefault()
            : candidateEdges.FirstOrDefault(e => string.Equals(e.SourceHandle, handle, StringComparison.OrdinalIgnoreCase));

        if (edge is null)
        {
            return null;
        }

        return _graph.Nodes.FirstOrDefault(n => n.Key == edge.Target);
    }

    /// <summary>All direct downstream nodes of `current`, regardless of handle - used by
    /// Parallel to fan out to every branch at once instead of following a single edge.</summary>
    public IReadOnlyList<WorkflowGraphNode> GetAllNext(WorkflowGraphNode current)
    {
        return _graph.Edges
            .Where(e => e.Source == current.Key)
            .Select(e => _graph.Nodes.FirstOrDefault(n => n.Key == e.Target))
            .Where(n => n is not null)
            .Select(n => n!)
            .ToList();
    }

    public WorkflowGraphNode? GetByKey(string key) => _graph.Nodes.FirstOrDefault(n => n.Key == key);
}
