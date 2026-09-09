using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (payload) with the org's stored Webhook URL and
/// optional auth header before delegating to WebhookConnector.</summary>
public class WebhookNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Webhook;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public WebhookNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var connectorId = await _db.Connectors
            .Where(c => c.Type == "Webhook" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (connectorId == 0)
        {
            return NodeResult.Fail("No built-in Webhook connector is configured.");
        }

        var webhookUrl = await _credentialStore.GetAsync(connectorId, "WebhookUrl", context.OrganizationId, cancellationToken);
        var authHeaderValue = await _credentialStore.GetAsync(connectorId, "AuthHeaderValue", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["webhookUrl"] = webhookUrl;
        nodeConfig["authHeaderValue"] = authHeaderValue;

        var connector = _connectorRegistry.Resolve("Webhook");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
