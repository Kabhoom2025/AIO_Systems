using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (content) with the org's stored Discord Incoming
/// Webhook URL before delegating to DiscordConnector.</summary>
public class DiscordMessageNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.DiscordMessage;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public DiscordMessageNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var discordConnectorId = await _db.Connectors
            .Where(c => c.Type == "Discord" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (discordConnectorId == 0)
        {
            return NodeResult.Fail("No built-in Discord connector is configured.");
        }

        var webhookUrl = await _credentialStore.GetAsync(discordConnectorId, "WebhookUrl", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["webhookUrl"] = webhookUrl;

        var connector = _connectorRegistry.Resolve("Discord");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
