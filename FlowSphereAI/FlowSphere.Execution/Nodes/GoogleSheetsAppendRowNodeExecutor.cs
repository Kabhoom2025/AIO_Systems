using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (spreadsheetId/sheetName/rowValuesJson) with the org's
/// stored Google service account JSON before delegating to GoogleSheetsConnector.</summary>
public class GoogleSheetsAppendRowNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.GoogleSheets;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public GoogleSheetsAppendRowNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var connectorId = await _db.Connectors
            .Where(c => c.Type == "GoogleSheets" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (connectorId == 0)
        {
            return NodeResult.Fail("No built-in Google Sheets connector is configured.");
        }

        var serviceAccountJson = await _credentialStore.GetAsync(connectorId, "ServiceAccountJson", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["serviceAccountJson"] = serviceAccountJson;

        var connector = _connectorRegistry.Resolve("GoogleSheets");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
