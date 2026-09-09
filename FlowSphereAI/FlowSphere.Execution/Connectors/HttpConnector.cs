using System.Text;
using System.Text.Json;

namespace FlowSphere.Execution.Connectors;

/// <summary>Generic REST connector. Node config shape: { "method": "GET|POST|PUT|DELETE",
/// "url": "https://...", "headers": { "K": "V" }, "body": "raw string, optional" }.</summary>
public class HttpConnector : IConnector
{
    public string Type => "Http";

    private readonly IHttpClientFactory _httpClientFactory;

    public HttpConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var method = root.TryGetProperty("method", out var methodEl) ? methodEl.GetString() ?? "GET" : "GET";
        var url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(url))
        {
            return ConnectorResult.Fail("HTTP node config is missing a 'url'.");
        }

        var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (root.TryGetProperty("headers", out var headersEl) && headersEl.ValueKind == JsonValueKind.Object)
        {
            foreach (var header in headersEl.EnumerateObject())
            {
                request.Headers.TryAddWithoutValidation(header.Name, header.Value.GetString());
            }
        }

        if (root.TryGetProperty("body", out var bodyEl) && bodyEl.ValueKind == JsonValueKind.String)
        {
            request.Content = new StringContent(bodyEl.GetString() ?? string.Empty, Encoding.UTF8, "application/json");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var output = JsonSerializer.Serialize(new
            {
                statusCode = (int)response.StatusCode,
                body = responseBody
            });

            return response.IsSuccessStatusCode
                ? ConnectorResult.Ok(output)
                : ConnectorResult.Fail($"HTTP request failed with status {(int)response.StatusCode}: {responseBody}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"HTTP request error: {ex.Message}");
        }
    }
}
