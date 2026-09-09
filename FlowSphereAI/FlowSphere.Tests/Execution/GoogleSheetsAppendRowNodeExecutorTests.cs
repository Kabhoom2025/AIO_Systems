using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class GoogleSheetsAppendRowNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "sheets1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoGoogleSheetsConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoGoogleSheetsConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("GoogleSheets", _ => ConnectorResult.Ok("{}")) });
        var executor = new GoogleSheetsAppendRowNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"spreadsheetId":"sheet1","rowValuesJson":"[]"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Google Sheets connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesServiceAccountJsonFromCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesServiceAccountJsonFromCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "GoogleSheets", Name = "Google Sheets", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ServiceAccountJson", 1, """{"client_email":"svc@acme.iam.gserviceaccount.com"}""");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("GoogleSheets", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"appended":true}""");
            })
        });
        var executor = new GoogleSheetsAppendRowNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"spreadsheetId":"sheet1","rowValuesJson":"[\"Alice\"]"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal(
            """{"client_email":"svc@acme.iam.gserviceaccount.com"}""",
            configDoc.RootElement.GetProperty("serviceAccountJson").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "GoogleSheets", Name = "Google Sheets", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "ServiceAccountJson", 1, """{"client_email":"svc@acme.iam.gserviceaccount.com"}""");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("GoogleSheets", _ => ConnectorResult.Fail("Google Sheets API error (403): permission denied"))
        });
        var executor = new GoogleSheetsAppendRowNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"spreadsheetId":"sheet1","rowValuesJson":"[]"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("permission denied", result.ErrorMessage);
    }
}
