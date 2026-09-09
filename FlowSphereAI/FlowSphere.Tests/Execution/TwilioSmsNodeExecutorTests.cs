using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class TwilioSmsNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "twilio1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoTwilioConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoTwilioConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Twilio", _ => ConnectorResult.Ok("{}")) });
        var executor = new TwilioSmsNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"to":"+15551234567","body":"hi"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Twilio connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Twilio", Name = "Twilio", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "AccountSid", 1, "AC123");
        credentialStore.Seed(connectorRow.Id, "AuthToken", 1, "token123");
        credentialStore.Seed(connectorRow.Id, "FromNumber", 1, "+15559999999");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Twilio", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"sent":true}""");
            })
        });
        var executor = new TwilioSmsNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"to":"+15551234567","body":"Deployment finished"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("AC123", configDoc.RootElement.GetProperty("accountSid").GetString());
        Assert.Equal("token123", configDoc.RootElement.GetProperty("authToken").GetString());
        Assert.Equal("+15559999999", configDoc.RootElement.GetProperty("fromNumber").GetString());
        Assert.Equal("+15551234567", configDoc.RootElement.GetProperty("to").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Twilio", Name = "Twilio", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "AccountSid", 1, "AC123");
        credentialStore.Seed(connectorRow.Id, "AuthToken", 1, "token123");
        credentialStore.Seed(connectorRow.Id, "FromNumber", 1, "+15559999999");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Twilio", _ => ConnectorResult.Fail("Twilio error (400): invalid number"))
        });
        var executor = new TwilioSmsNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"to":"+1555","body":"hi"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("invalid number", result.ErrorMessage);
    }
}
