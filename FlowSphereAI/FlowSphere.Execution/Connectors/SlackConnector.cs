using System.Net.Http.Json;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Posts to a Slack Incoming Webhook - the simplest Slack integration (a per-workspace
/// URL that accepts a JSON payload and posts it to whatever channel the webhook was created
/// for). Node config shape: { "text": "<message>", "channel": "<optional #channel override>" }.
/// The webhook URL itself is a per-organization secret (ConnectorCredential "WebhookUrl"),
/// resolved by SlackMessageNodeExecutor before invoking this connector - Slack's webhook URL
/// IS the authentication, there is no separate API key.</summary>
public class SlackConnector : IConnector
{
    public string Type => "Slack";

    private readonly IHttpClientFactory _httpClientFactory;

    public SlackConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var webhookUrl = root.TryGetProperty("webhookUrl", out var urlEl) ? urlEl.GetString() : null;
        var text = root.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
        var channel = root.TryGetProperty("channel", out var channelEl) ? channelEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return ConnectorResult.Fail("No Slack webhook URL is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return ConnectorResult.Fail("Slack Message node config is missing 'text'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");

        try
        {
            var payload = string.IsNullOrWhiteSpace(channel)
                ? (object)new { text }
                : new { text, channel };

            var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Slack webhook error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { posted = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Slack webhook request error: {ex.Message}");
        }
    }
}
