using FlowSphere.Execution.Connectors;

namespace FlowSphere.Tests.TestUtilities;

public class FakeConnector : IConnector
{
    private readonly Func<ConnectorInvocation, ConnectorResult> _behavior;

    public FakeConnector(string type, Func<ConnectorInvocation, ConnectorResult> behavior)
    {
        Type = type;
        _behavior = behavior;
    }

    public string Type { get; }

    public Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        return Task.FromResult(_behavior(invocation));
    }
}
