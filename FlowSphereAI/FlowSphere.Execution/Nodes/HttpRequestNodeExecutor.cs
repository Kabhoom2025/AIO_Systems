using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;

namespace FlowSphere.Execution.Nodes;

public class HttpRequestNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.HttpRequest;

    private readonly ConnectorRegistry _connectorRegistry;

    public HttpRequestNodeExecutor(ConnectorRegistry connectorRegistry)
    {
        _connectorRegistry = connectorRegistry;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var connector = _connectorRegistry.Resolve("Http");
        var result = await connector.InvokeAsync(new ConnectorInvocation(context.ConfigJson, context.InputJson), cancellationToken);

        return result.Success
            ? NodeResult.Ok(result.OutputJson)
            : NodeResult.Fail(result.ErrorMessage!);
    }
}
