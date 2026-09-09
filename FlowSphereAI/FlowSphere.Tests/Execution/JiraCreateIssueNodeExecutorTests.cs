using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using FlowSphere.Execution.Connectors;
using FlowSphere.Execution.Nodes;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class JiraCreateIssueNodeExecutorTests
{
    private static NodeExecutionContext Context(string configJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "jira1",
        ConfigJson = configJson,
        InputJson = "{}",
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task ExecuteAsync_NoJiraConnectorConfigured_ReturnsFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_NoJiraConnectorConfigured_ReturnsFailure));

        var registry = new ConnectorRegistry(new IConnector[] { new FakeConnector("Jira", _ => ConnectorResult.Ok("{}")) });
        var executor = new JiraCreateIssueNodeExecutor(registry, new FakeConnectorCredentialStore(), db);

        var result = await executor.ExecuteAsync(Context("""{"projectKey":"OPS","summary":"Fix it"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("No built-in Jira connector", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ResolvesCredentials_AndMergesIntoConnectorInvocation));
        var connectorRow = new Connector { Type = "Jira", Name = "Jira", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "BaseUrl", 1, "https://acme.atlassian.net");
        credentialStore.Seed(connectorRow.Id, "Email", 1, "admin@acme.com");
        credentialStore.Seed(connectorRow.Id, "ApiToken", 1, "token123");

        string? capturedConfigJson = null;
        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Jira", invocation =>
            {
                capturedConfigJson = invocation.ConfigJson;
                return ConnectorResult.Ok("""{"created":true}""");
            })
        });
        var executor = new JiraCreateIssueNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"projectKey":"OPS","summary":"Fix it"}"""), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(capturedConfigJson);

        using var configDoc = System.Text.Json.JsonDocument.Parse(capturedConfigJson!);
        Assert.Equal("https://acme.atlassian.net", configDoc.RootElement.GetProperty("baseUrl").GetString());
        Assert.Equal("admin@acme.com", configDoc.RootElement.GetProperty("email").GetString());
        Assert.Equal("token123", configDoc.RootElement.GetProperty("apiToken").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ConnectorFails_PropagatesErrorMessage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(ExecuteAsync_ConnectorFails_PropagatesErrorMessage));
        var connectorRow = new Connector { Type = "Jira", Name = "Jira", OrganizationId = null };
        db.Connectors.Add(connectorRow);
        await db.SaveChangesAsync();

        var credentialStore = new FakeConnectorCredentialStore();
        credentialStore.Seed(connectorRow.Id, "BaseUrl", 1, "https://acme.atlassian.net");
        credentialStore.Seed(connectorRow.Id, "Email", 1, "admin@acme.com");
        credentialStore.Seed(connectorRow.Id, "ApiToken", 1, "token123");

        var registry = new ConnectorRegistry(new IConnector[]
        {
            new FakeConnector("Jira", _ => ConnectorResult.Fail("Jira API error (400): project does not exist"))
        });
        var executor = new JiraCreateIssueNodeExecutor(registry, credentialStore, db);

        var result = await executor.ExecuteAsync(Context("""{"projectKey":"MISSING","summary":"Fix it"}"""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("project does not exist", result.ErrorMessage);
    }
}
