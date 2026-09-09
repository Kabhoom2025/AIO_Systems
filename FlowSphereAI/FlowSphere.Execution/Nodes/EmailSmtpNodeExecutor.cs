using System.Text.Json;
using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (to/subject/body) with the org's stored SMTP
/// credentials (Host/Port/Username/Password, resolved via IConnectorCredentialStore) before
/// delegating to SmtpConnector - keeps the connector itself credential-lookup-free.</summary>
public class EmailSmtpNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.EmailSmtp;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public EmailSmtpNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var smtpConnectorId = await _db.Connectors
            .Where(c => c.Type == "Smtp" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (smtpConnectorId == 0)
        {
            return NodeResult.Fail("No built-in Smtp connector is configured.");
        }

        var host = await _credentialStore.GetAsync(smtpConnectorId, "Host", context.OrganizationId, cancellationToken);
        var port = await _credentialStore.GetAsync(smtpConnectorId, "Port", context.OrganizationId, cancellationToken);
        var username = await _credentialStore.GetAsync(smtpConnectorId, "Username", context.OrganizationId, cancellationToken);
        var password = await _credentialStore.GetAsync(smtpConnectorId, "Password", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["smtpHost"] = host;
        nodeConfig["smtpPort"] = int.TryParse(port, out var p) ? p : 587;
        nodeConfig["smtpUsername"] = username;
        nodeConfig["smtpPassword"] = password;

        var connector = _connectorRegistry.Resolve("Smtp");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
