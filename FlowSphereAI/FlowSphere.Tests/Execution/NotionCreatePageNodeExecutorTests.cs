using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class NotionCreatePageNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "notion1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoNotionConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoNotionConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Notion", _ => ConnectorResult.Ok("{}")) });
        var executor = new NotionCreatePageNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"databaseId":"db1","title":"New task"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Notion connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesApiKeyFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesApiKeyFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Notion", Name = "Notion", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "secret_abc");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Notion", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"created":true}""");
            })
        });
        var executor = new NotionCreatePageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"databaseId":"db1","title":"New task"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("secret_abc", configDoc.RootElement.GetProperty("apiKey").GetString());
        Assert.Equal("db1", configDoc.RootElement.GetProperty("databaseId").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Notion", Name = "Notion", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "secret_abc");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Notion", _ => ConnectorResult.Fail("Notion API error (404): database not found"))
        });
        var executor = new NotionCreatePageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"databaseId":"missing","title":"New task"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("database not found", result.ErrorMessage);
    }
}
