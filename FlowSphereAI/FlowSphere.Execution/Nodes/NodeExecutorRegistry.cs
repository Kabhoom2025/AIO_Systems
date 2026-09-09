using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Populated from all registered INodeExecutor implementations via DI - adding a node
/// type means implementing INodeExecutor and registering it, no engine changes.</summary>
public class NodeExecutorRegistry : INodeExecutorRegistry
{
    private readonly Dictionary<WorkflowNodeType, INodeExecutor> _executorsByType;

    public NodeExecutorRegistry(IEnumerable<INodeExecutor> executors)
    {
        _executorsByType = executors.ToDictionary(e => e.SupportedType);
    }

    public INodeExecutor Resolve(WorkflowNodeType nodeType)
    {
        if (!_executorsByType.TryGetValue(nodeType, out var executor))
        {
            throw new InvalidOperationException($"No node executor registered for type '{nodeType}'.");
        }

        return executor;
    }
}
