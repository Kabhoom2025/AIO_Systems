using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Landing point for a failed node's "error" handle (see
/// WorkflowExecutionEngine.RunAsync's failure branch, which looks for an outgoing "error" edge
/// before giving up on the whole execution). Receives the upstream failure as InputJson
/// (an { error, failedNodeKey } payload built by the engine) and simply passes it through so
/// the workflow can continue - e.g. into a notification node - instead of failing outright.</summary>
public class ExceptionNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Exception;

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken) =>
        Task.FromResult(NodeResult.Ok(context.InputJson));
}
