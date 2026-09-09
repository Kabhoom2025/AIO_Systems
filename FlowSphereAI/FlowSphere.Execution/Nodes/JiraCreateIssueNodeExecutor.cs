using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

/// <summary>Merges the node's own config (projectKey/summary/description/issueType) with the
/// org's stored Jira credentials (BaseUrl/Email/ApiToken) before delegating to JiraConnector.</summary>
public class JiraCreateIssueNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.Jira;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public JiraCreateIssueNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var connectorId = await _db.Connectors
            .Where(c => c.Type == "Jira" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (connectorId == 0)
        {
            return NodeResult.Fail("No built-in Jira connector is configured.");
        }

        var baseUrl = await _credentialStore.GetAsync(connectorId, "BaseUrl", context.OrganizationId, cancellationToken);
        var email = await _credentialStore.GetAsync(connectorId, "Email", context.OrganizationId, cancellationToken);
        var apiToken = await _credentialStore.GetAsync(connectorId, "ApiToken", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["baseUrl"] = baseUrl;
        nodeConfig["email"] = email;
        nodeConfig["apiToken"] = apiToken;

        var connector = _connectorRegistry.Resolve("Jira");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
