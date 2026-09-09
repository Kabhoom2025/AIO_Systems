using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class SlackMessageNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "slack1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoSlackConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoSlackConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Slack", _ => ConnectorResult.Ok("{}")) });
        var executor = new SlackMessageNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"text":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Slack connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Slack", Name = "Slack", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://hooks.slack.com/services/fake/webhook");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Slack", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"posted":true}""");
            })
        });
        var executor = new SlackMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"text":"Deployment finished"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("https://hooks.slack.com/services/fake/webhook", configDoc.RootElement.GetProperty("webhookUrl").GetString());
        Assert.Equal("Deployment finished", configDoc.RootElement.GetProperty("text").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Slack", Name = "Slack", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://hooks.slack.com/services/fake/webhook");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Slack", _ => ConnectorResult.Fail("Slack webhook error (404): no_service"))
        });
        var executor = new SlackMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"text":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("no_service", result.ErrorMessage);
    }
}
