using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (to/body) with the org's stored Twilio credentials
/// (AccountSid/AuthToken/FromNumber) before delegating to TwilioConnector.</summary>
public class TwilioSmsNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Twilio;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public TwilioSmsNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var connectorId = await _db.Connectors
            .Where(c => c.Type == "Twilio" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (connectorId == 0)
        {
            return NodeResult.Fail("No built-in Twilio connector is configured.");
        }

        var accountSid = await _credentialStore.GetAsync(connectorId, "AccountSid", context.OrganizationId, cancellationToken);
        var authToken = await _credentialStore.GetAsync(connectorId, "AuthToken", context.OrganizationId, cancellationToken);
        var fromNumber = await _credentialStore.GetAsync(connectorId, "FromNumber", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["accountSid"] = accountSid;
        nodeConfig["authToken"] = authToken;
        nodeConfig["fromNumber"] = fromNumber;

        var connector = _connectorRegistry.Resolve("Twilio");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
