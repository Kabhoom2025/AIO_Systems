using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Tests.TestUtilities;

public class FakeNodeExecutor : INodeExecutor
{
    private readonly Func<NodeExecutionContext, CancellationToken, Task<NodeResult>> _behavior;

    public FakeNodeExecutor(WorkflowNodeType supportedType, Func<NodeExecutionContext, NodeResult> behavior)
        : this(supportedType, (context, _) => Task.FromResult(behavior(context)))
    {
    }

    public FakeNodeExecutor(WorkflowNodeType supportedType, Func<NodeExecutionContext, CancellationToken, Task<NodeResult>> behavior)
    {
        SupportedType = supportedType;
        _behavior = behavior;
    }

    public WorkflowNodeType SupportedType { get; }

    public Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        return _behavior(context, cancellationToken);
    }
}
