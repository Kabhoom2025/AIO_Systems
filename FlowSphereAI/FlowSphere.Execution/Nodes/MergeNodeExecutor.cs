using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Join point for a Parallel node's branches. The engine already combines the
/// branches' outputs into this node's InputJson before executing it (see
/// WorkflowExecutionEngine.RunParallelAsync) - this executor just passes that combined payload
/// through unchanged, so it shows up as a normal, inspectable step in the execution timeline.</summary>
public class MergeNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Merge;

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken) =>
        Task.FromResult(NodeResult.Ok(context.InputJson));
}
