using System.Net.Http.Json;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Posts to a Microsoft Teams Incoming Webhook (an Office 365 Connector configured on
/// a channel) - same shape as Slack's: a per-workspace/channel URL that accepts a JSON payload.
/// Node config shape: { "text": "<message>", "title": "<optional card title>" }. The minimal
/// payload Teams accepts is { "text": "..." }; a "title" is added to the payload when present
/// since Teams renders it as a bolded card heading above the text.</summary>
public class TeamsConnector : IConnector
{
    public string Type => "Teams";

    private readonly IHttpClientFactory _httpClientFactory;

    public TeamsConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var webhookUrl = root.TryGetProperty("webhookUrl", out var urlEl) ? urlEl.GetString() : null;
        var text = root.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
        var title = root.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return ConnectorResult.Fail("No Teams webhook URL is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return ConnectorResult.Fail("Teams Message node config is missing 'text'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");

        try
        {
            var payload = string.IsNullOrWhiteSpace(title)
                ? (object)new { text }
                : new { title, text };

            var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Teams webhook error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { posted = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Teams webhook request error: {ex.Message}");
        }
    }
}
