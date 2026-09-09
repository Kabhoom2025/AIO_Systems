using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Execution.Nodes;

/// <summary>Pass-through: seeds the execution context with the execution's InputPayloadJson so
/// downstream nodes can reference it.</summary>
public class TriggerWebhookNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Trigger;

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(NodeResult.Ok(context.InputJson));
    }
}
