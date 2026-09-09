using System.Text;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Posts a JSON payload to an arbitrary, org-configured URL - a lighter sibling to
/// HttpConnector aimed at "send this workflow's data to my own endpoint" rather than calling a
/// specific third-party REST API. Unlike HttpConnector (where the URL is per-node config), the
/// URL here is a per-organization credential ("WebhookUrl"), matching the Slack/Teams/Discord
/// webhook pattern, plus an optional bearer-style "AuthHeaderValue" credential for endpoints that
/// expect an Authorization header. Node config shape: { "payload": "&lt;raw JSON string&gt;" }.</summary>
public class WebhookConnector : IConnector
{
    public string Type => "Webhook";

    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var webhookUrl = root.TryGetProperty("webhookUrl", out var urlEl) ? urlEl.GetString() : null;
        var authHeaderValue = root.TryGetProperty("authHeaderValue", out var authEl) ? authEl.GetString() : null;
        var payload = root.TryGetProperty("payload", out var payloadEl) ? payloadEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return ConnectorResult.Fail("No Webhook URL is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return ConnectorResult.Fail("Webhook node config is missing 'payload'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };

        if (!string.IsNullOrWhiteSpace(authHeaderValue))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authHeaderValue);
        }

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Webhook error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { posted = true, statusCode = (int)response.StatusCode, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Webhook request error: {ex.Message}");
        }
    }
}
