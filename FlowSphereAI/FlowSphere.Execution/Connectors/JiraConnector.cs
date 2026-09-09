using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Creates an issue in a Jira Cloud project via the REST API v3 (Basic Auth with
/// email:apiToken - Jira Cloud's standard API token flow, no OAuth needed). Node config shape: {
/// "baseUrl", "email", "apiToken" - merged in by JiraCreateIssueNodeExecutor from the org's
/// stored credentials - plus "projectKey", "summary", "description" (optional) and "issueType"
/// (optional, defaults to "Task") from the node itself. Description is wrapped in the minimal
/// Atlassian Document Format the v3 API requires for rich-text fields.</summary>
public class JiraConnector : IConnector
{
    public string Type => "Jira";

    private readonly IHttpClientFactory _httpClientFactory;

    public JiraConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var baseUrl = root.TryGetProperty("baseUrl", out var baseUrlEl) ? baseUrlEl.GetString() : null;
        var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
        var apiToken = root.TryGetProperty("apiToken", out var tokenEl) ? tokenEl.GetString() : null;
        var projectKey = root.TryGetProperty("projectKey", out var projectEl) ? projectEl.GetString() : null;
        var summary = root.TryGetProperty("summary", out var summaryEl) ? summaryEl.GetString() : null;
        var description = root.TryGetProperty("description", out var descEl) ? descEl.GetString() : null;
        var issueType = root.TryGetProperty("issueType", out var typeEl) ? typeEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(apiToken))
        {
            return ConnectorResult.Fail("No Jira credentials are configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(projectKey) || string.IsNullOrWhiteSpace(summary))
        {
            return ConnectorResult.Fail("Jira node config is missing 'projectKey' or 'summary'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/rest/api/3/issue")
        {
            Content = JsonContent.Create(new
            {
                fields = new
                {
                    project = new { key = projectKey },
                    summary,
                    issuetype = new { name = string.IsNullOrWhiteSpace(issueType) ? "Task" : issueType },
                    description = string.IsNullOrWhiteSpace(description) ? null : BuildAdfDocument(description),
                },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{apiToken}")));

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Jira API error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { created = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Jira request error: {ex.Message}");
        }
    }

    private static object BuildAdfDocument(string text) => new
    {
        type = "doc",
        version = 1,
        content = new[]
        {
            new
            {
                type = "paragraph",
                content = new[] { new { type = "text", text } },
            },
        },
    };
}
