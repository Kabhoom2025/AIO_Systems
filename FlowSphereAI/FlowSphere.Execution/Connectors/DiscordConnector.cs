using System.Net.Http.Json;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Posts to a Discord Incoming Webhook (per-channel URL created via Channel Settings ->
/// Integrations -> Webhooks). Node config shape: { "content": "<message>" }. Discord's webhook
/// API accepts { "content": "..." } as the minimal payload.</summary>
public class DiscordConnector : IConnector
{
    public string Type => "Discord";

    private readonly IHttpClientFactory _httpClientFactory;

    public DiscordConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var webhookUrl = root.TryGetProperty("webhookUrl", out var urlEl) ? urlEl.GetString() : null;
        var content = root.TryGetProperty("content", out var contentEl) ? contentEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return ConnectorResult.Fail("No Discord webhook URL is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ConnectorResult.Fail("Discord Message node config is missing 'content'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");

        try
        {
            var response = await client.PostAsJsonAsync(webhookUrl, new { content }, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            // Discord returns 204 No Content on success (not 200), unlike Slack/Teams.
            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Discord webhook error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { posted = true }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Discord webhook request error: {ex.Message}");
        }
    }
}
