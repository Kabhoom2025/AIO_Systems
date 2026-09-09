using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Copilot;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class WorkflowGraphGeneratorTests
{
    private const string ValidGraphJson = """
        {"nodes":[{"key":"trigger1","type":"Trigger","config":{}},{"key":"http1","type":"HttpRequest","config":{"method":"GET","url":"https://example.com"}}],"edges":[{"source":"trigger1","target":"http1"}]}
        """;

    [Fact]
    public async Task GenerateGraphJsonAsync_NoConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_NoConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("OpenAi", _ => ConnectorResult.Ok("{}")) });
        var generator = new WorkflowGraphGenerator(registry, new FakeConnectorCredentialStore(), db);

        var result = await generator.GenerateGraphJsonAsync("build me a workflow", null, 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("No OpenAI connector", result.Error!.Message);
    }

    [Fact]
    public async Task GenerateGraphJsonAsync_NoApiKeyConfigured_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_NoApiKeyConfigured_ReturnsValidationFailure));
        db.Connectors.Add(new Connector { Type = "OpenAi", Name = "OpenAI", OrganizationId = null });
        await db.SaveChangesAsync();

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("OpenAi", _ => ConnectorResult.Ok("{}")) });
        var generator = new WorkflowGraphGenerator(registry, new FakeConnectorCredentialStore(), db);

        var result = await generator.GenerateGraphJsonAsync("build me a workflow", null, 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.ValidationErrors);
        Assert.Contains("apiKey", result.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task GenerateGraphJsonAsync_ConnectorInvocationFails_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_ConnectorInvocationFails_ReturnsFailure));
        var connectorRow = new Connector { Type = "OpenAi", Name = "OpenAI", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "sk-fake-key");

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("OpenAi", _ => ConnectorResult.Fail("rate limited")) });
        var generator = new WorkflowGraphGenerator(registry, credentialStore, db);

        var result = await generator.GenerateGraphJsonAsync("build me a workflow", null, 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("rate limited", result.Error!.Message);
    }

    [Fact]
    public async Task GenerateGraphJsonAsync_ValidCompletionWrappedInCodeFences_StripsAndSucceeds()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_ValidCompletionWrappedInCodeFences_StripsAndSucceeds));
        var connectorRow = new Connector { Type = "OpenAi", Name = "OpenAI", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "sk-fake-key");

        var fencedCompletion = $"```json\n{ValidGraphJson}\n```";
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("OpenAi", _ => ConnectorResult.Ok($$"""{"completion": {{System.Text.Json.JsonSerializer.Serialize(fencedCompletion)}} }"""))
        });
        var generator = new WorkflowGraphGenerator(registry, credentialStore, db);

        var result = await generator.GenerateGraphJsonAsync("build me a workflow", null, 1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("```", result.Value);
        Assert.Contains("\"Trigger\"", result.Value);
    }

    [Fact]
    public async Task GenerateGraphJsonAsync_CompletionMissingTriggerNode_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_CompletionMissingTriggerNode_ReturnsFailure));
        var connectorRow = new Connector { Type = "OpenAi", Name = "OpenAI", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "sk-fake-key");

        const string graphWithoutTrigger = """{"nodes":[{"key":"http1","type":"HttpRequest","config":{}}],"edges":[]}""";
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("OpenAi", _ => ConnectorResult.Ok($$"""{"completion": {{System.Text.Json.JsonSerializer.Serialize(graphWithoutTrigger)}} }"""))
        });
        var generator = new WorkflowGraphGenerator(registry, credentialStore, db);

        var result = await generator.GenerateGraphJsonAsync("build me a workflow", null, 1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("exactly one Trigger node", result.Error!.Message);
    }

    [Fact]
    public async Task GenerateGraphJsonAsync_WithCurrentGraph_IncludesItInThePromptSentToTheConnector()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(GenerateGraphJsonAsync_WithCurrentGraph_IncludesItInThePromptSentToTheConnector));
        var connectorRow = new Connector { Type = "OpenAi", Name = "OpenAI", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "sk-fake-key");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("OpenAi", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok($$"""{"completion": {{System.Text.Json.JsonSerializer.Serialize(ValidGraphJson)}} }""");
            })
        });
        var generator = new WorkflowGraphGenerator(registry, credentialStore, db);

        const string existingGraph = """{"nodes":[{"key":"trigger1","type":"Trigger","config":{}}],"edges":[]}""";
        var result = await generator.GenerateGraphJsonAsync("add an HTTP request node", existingGraph, 1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedConfigJson);

        // capturedConfigJson wraps the actual prompt as a JSON string value, so its special
        // characters (quotes from the embedded graph JSON) are escaped - parse it back out
        // instead of substring-matching the raw escaped text.
        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        var sentPrompt = configDoc.RootElement.GetProperty("prompt").GetString();

        Assert.Contains(existingGraph, sentPrompt);
        Assert.Contains("add an HTTP request node", sentPrompt);
        Assert.Contains("Modify that graph", sentPrompt);
    }
}
