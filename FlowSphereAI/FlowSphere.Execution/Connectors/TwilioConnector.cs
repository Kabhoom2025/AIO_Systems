using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Sends an SMS via Twilio's REST API (Basic Auth with AccountSid:AuthToken, form-
/// encoded body). Node config shape: { "accountSid", "authToken", "fromNumber" - merged in by
/// TwilioSmsNodeExecutor from the org's stored credentials - plus "to" and "body" from the node
/// itself }.</summary>
public class TwilioConnector : IConnector
{
    public string Type => "Twilio";

    private readonly IHttpClientFactory _httpClientFactory;

    public TwilioConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var accountSid = root.TryGetProperty("accountSid", out var sidEl) ? sidEl.GetString() : null;
        var authToken = root.TryGetProperty("authToken", out var tokenEl) ? tokenEl.GetString() : null;
        var fromNumber = root.TryGetProperty("fromNumber", out var fromEl) ? fromEl.GetString() : null;
        var to = root.TryGetProperty("to", out var toEl) ? toEl.GetString() : null;
        var body = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(fromNumber))
        {
            return ConnectorResult.Fail("No Twilio credentials are configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(body))
        {
            return ConnectorResult.Fail("Twilio SMS node config is missing 'to' or 'body'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(
            HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = to,
                ["From"] = fromNumber,
                ["Body"] = body,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountSid}:{authToken}")));

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Twilio error ({(int)response.StatusCode}): {responseBody}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { sent = true, response = responseBody }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Twilio request error: {ex.Message}");
        }
    }
}
