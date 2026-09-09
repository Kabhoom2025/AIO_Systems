using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class DiscordMessageNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "discord1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoDiscordConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoDiscordConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Discord", _ => ConnectorResult.Ok("{}")) });
        var executor = new DiscordMessageNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"content":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Discord connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Discord", Name = "Discord", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://discord.com/api/webhooks/fake");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Discord", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"posted":true}""");
            })
        });
        var executor = new DiscordMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"content":"Build succeeded"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("https://discord.com/api/webhooks/fake", configDoc.RootElement.GetProperty("webhookUrl").GetString());
        Assert.Equal("Build succeeded", configDoc.RootElement.GetProperty("content").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Discord", Name = "Discord", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://discord.com/api/webhooks/fake");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Discord", _ => ConnectorResult.Fail("Discord webhook error (404): unknown webhook"))
        });
        var executor = new DiscordMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"content":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("unknown webhook", result.ErrorMessage);
    }
}
