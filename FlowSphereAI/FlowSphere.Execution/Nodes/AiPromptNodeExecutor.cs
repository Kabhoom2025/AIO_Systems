using System.Text.Json.Nodes;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using FlowSphere.Execution.Connectors;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Execution.Nodes;

public class AiPromptNodeExecutor : INodeExecutor
{
    public WorkflowNodeType SupportedType => WorkflowNodeType.AiPrompt;

    private readonly ConnectorRegistry _connectorRegistry;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IApplicationDbContext _db;

    public AiPromptNodeExecutor(ConnectorRegistry connectorRegistry, IConnectorCredentialStore credentialStore, IApplicationDbContext db)
    {
        _connectorRegistry = connectorRegistry;
        _credentialStore = credentialStore;
        _db = db;
    }

    public async Task<NodeResult> ExecuteAsync(NodeExecutionContext context, CancellationToken cancellationToken)
    {
        var openAiConnectorId = await _db.Connectors
            .Where(c => c.Type == "OpenAi" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (openAiConnectorId == 0)
        {
            return NodeResult.Fail("No built-in OpenAi connector is configured.");
        }

        var apiKey = await _credentialStore.GetAsync(openAiConnectorId, "ApiKey", context.OrganizationId, cancellationToken);
        var baseUrl = await _credentialStore.GetAsync(openAiConnectorId, "BaseUrl", context.OrganizationId, cancellationToken);
        var orgModel = await _credentialStore.GetAsync(openAiConnectorId, "Model", context.OrganizationId, cancellationToken);

        var nodeConfig = JsonNode.Parse(context.ConfigJson)?.AsObject() ?? new JsonObject();
        nodeConfig["apiKey"] = apiKey;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            nodeConfig["baseUrl"] = baseUrl;
        }
        if (!string.IsNullOrWhiteSpace(orgModel))
        {
            // The org's configured Model credential wins over whatever model string is baked
            // into the node's own config (e.g. Copilot-generated nodes always default to
            // "gpt-4o-mini", which would 404 against a non-OpenAI BaseUrl like Groq/xAI).
            // BaseUrl/ApiKey/Model are all organization-wide settings, not per-node, so this
            // keeps them consistent - configure the provider once, every AiPrompt node uses it.
            nodeConfig["model"] = orgModel;
        }

        var connector = _connectorRegistry.Resolve("OpenAi");
        var result = await connector.InvokeAsync(new ConnectorInvocation(nodeConfig.ToJsonString(), context.InputJson), cancellationToken);

        return result.Success ? NodeResult.Ok(result.OutputJson) : NodeResult.Fail(result.ErrorMessage!);
    }
}
