using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlowSphere.Execution.Connectors;

/// <summary>Creates a record in an Airtable base/table via Airtable's REST API (Bearer auth with
/// a Personal Access Token). Node config shape: { "apiKey", "baseId" - merged in by
/// AirtableCreateRecordNodeExecutor from the org's stored credentials - plus "table" and
/// "fieldsJson" (a raw JSON object string, e.g. {"Name":"Alice","Status":"Active"}) from the node
/// itself.</summary>
public class AirtableConnector : IConnector
{
    public string Type => "Airtable";

    private readonly IHttpClientFactory _httpClientFactory;

    public AirtableConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var apiKey = root.TryGetProperty("apiKey", out var keyEl) ? keyEl.GetString() : null;
        var baseId = root.TryGetProperty("baseId", out var baseEl) ? baseEl.GetString() : null;
        var table = root.TryGetProperty("table", out var tableEl) ? tableEl.GetString() : null;
        var fieldsJson = root.TryGetProperty("fieldsJson", out var fieldsEl) ? fieldsEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseId))
        {
            return ConnectorResult.Fail("No Airtable credentials are configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(fieldsJson))
        {
            return ConnectorResult.Fail("Airtable node config is missing 'table' or 'fieldsJson'.");
        }

        JsonNode? fields;
        try
        {
            fields = JsonNode.Parse(fieldsJson);
        }
        catch (JsonException)
        {
            return ConnectorResult.Fail("Airtable node config's 'fieldsJson' is not valid JSON.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(
            HttpMethod.Post, $"https://api.airtable.com/v0/{baseId}/{Uri.EscapeDataString(table)}")
        {
            Content = JsonContent.Create(new { fields }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Airtable API error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { created = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Airtable request error: {ex.Message}");
        }
    }
}
