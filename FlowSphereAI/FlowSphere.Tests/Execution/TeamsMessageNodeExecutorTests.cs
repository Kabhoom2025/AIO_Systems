using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class TeamsMessageNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "teams1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoTeamsConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoTeamsConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Teams", _ => ConnectorResult.Ok("{}")) });
        var executor = new TeamsMessageNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"text":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Teams connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesWebhookUrlFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Teams", Name = "Microsoft Teams", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://outlook.office.com/webhook/fake");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Teams", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"posted":true}""");
            })
        });
        var executor = new TeamsMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"text":"Build succeeded"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("https://outlook.office.com/webhook/fake", configDoc.RootElement.GetProperty("webhookUrl").GetString());
        Assert.Equal("Build succeeded", configDoc.RootElement.GetProperty("text").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Teams", Name = "Microsoft Teams", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "WebhookUrl", 1, "https://outlook.office.com/webhook/fake");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Teams", _ => ConnectorResult.Fail("Teams webhook error (410): webhook disabled"))
        });
        var executor = new TeamsMessageNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"text":"hello"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("webhook disabled", result.ErrorMessage);
    }
}
