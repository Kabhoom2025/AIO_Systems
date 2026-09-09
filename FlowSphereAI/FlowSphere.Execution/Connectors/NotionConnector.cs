using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Creates a page in a Notion database via the Notion API. Node config shape: {
/// "apiKey" - merged in by NotionCreatePageNodeExecutor from the org's stored Integration token
/// - plus "databaseId" and "title" from the node itself. Assumes the target database's title
/// property is named "Name", which is Notion's default for every new database.</summary>
public class NotionConnector : IConnector
{
    public string Type => "Notion";

    private const string NotionVersion = "2022-06-28";

    private readonly IHttpClientFactory _httpClientFactory;

    public NotionConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var apiKey = root.TryGetProperty("apiKey", out var keyEl) ? keyEl.GetString() : null;
        var databaseId = root.TryGetProperty("databaseId", out var dbEl) ? dbEl.GetString() : null;
        var title = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ConnectorResult.Fail("No Notion integration token is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(databaseId) || string.IsNullOrWhiteSpace(title))
        {
            return ConnectorResult.Fail("Notion node config is missing 'databaseId' or 'title'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.notion.com/v1/pages")
        {
            Content = JsonContent.Create(new
            {
                parent = new { database_id = databaseId },
                properties = new
                {
                    Name = new
                    {
                        title = new[] { new { text = new { content = title } } },
                    },
                },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.TryAddWithoutValidation("Notion-Version", NotionVersion);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Notion API error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { created = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Notion request error: {ex.Message}");
        }
    }
}
