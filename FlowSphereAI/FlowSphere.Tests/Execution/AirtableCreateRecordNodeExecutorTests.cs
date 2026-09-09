using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class AirtableCreateRecordNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "airtable1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoAirtableConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoAirtableConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Airtable", _ => ConnectorResult.Ok("{}")) });
        var executor = new AirtableCreateRecordNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"table":"Tasks","fieldsJson":"{}"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Airtable connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Airtable", Name = "Airtable", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "pat_abc");
        credentialStore.Seed(connectorRow.Id, "BaseId", 1, "appXYZ");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Airtable", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"created":true}""");
            })
        });
        var executor = new AirtableCreateRecordNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"table":"Tasks","fieldsJson":"{\"Name\":\"Alice\"}"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("pat_abc", configDoc.RootElement.GetProperty("apiKey").GetString());
        Assert.Equal("appXYZ", configDoc.RootElement.GetProperty("baseId").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Airtable", Name = "Airtable", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ApiKey", 1, "pat_abc");
        credentialStore.Seed(connectorRow.Id, "BaseId", 1, "appXYZ");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Airtable", _ => ConnectorResult.Fail("Airtable API error (404): table not found"))
        });
        var executor = new AirtableCreateRecordNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"table":"Missing","fieldsJson":"{}"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("table not found", result.ErrorMessage);
    }
}
