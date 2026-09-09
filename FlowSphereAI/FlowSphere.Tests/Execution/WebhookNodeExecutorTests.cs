using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class WebhookNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "webhook1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoWebhookConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoWebhookConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Webhook", _ => ConnectorResult.Ok("{}")) });
        var executor = new WebhookNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"payload":"{}"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Webhook connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesUrlAndAuthHeaderFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesUrlAndAuthHeaderFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Webhook", Name = "Webhook", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://example.com/hook");
        credentialStore.Seed(connectorRow.Id, "AuthHeaderValue", 1, "Bearer secret");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Webhook", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"posted":true}""");
            })
        });
        var executor = new WebhookNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"payload":"{\"a\":1}"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("https://example.com/hook", configDoc.RootElement.GetProperty("webhookUrl").GetString());
        Assert.Equal("Bearer secret", configDoc.RootElement.GetProperty("authHeaderValue").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Webhook", Name = "Webhook", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://example.com/hook");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Webhook", _ => ConnectorResult.Fail("Webhook error (500): boom"))
        });
        var executor = new WebhookNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"payload":"{}"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("boom", result.ErrorMessage);
    }
}
