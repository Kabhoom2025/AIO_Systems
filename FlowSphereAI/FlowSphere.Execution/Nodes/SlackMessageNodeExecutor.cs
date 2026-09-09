using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (text/channel) with the org's stored Slack Incoming
/// Webhook URL before delegating to SlackConnector.</summary>
public class SlackMessageNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.SlackMessage;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public SlackMessageNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var slackConnectorId = await _db.Connectors
            .Where(c => c.Type == "Slack" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (slackConnectorId == 0)
        {
            return NodeResult.Fail("No built-in Slack connector is configured.");
        }

        var webhookUrl = await _credentialStore.GetAsync(slackConnectorId, "WebhookUrl", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["webhookUrl"] = webhookUrl;

        var connector = _connectorRegistry.Resolve("Slack");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
