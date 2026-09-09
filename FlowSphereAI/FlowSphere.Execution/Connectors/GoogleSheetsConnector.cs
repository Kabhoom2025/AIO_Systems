using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowSphere.Execution.Connectors;

/// <summary>Appends a row to a Google Sheet using a service account - no OAuth consent/redirect
/// flow, since service accounts authenticate themselves via a self-signed JWT exchanged for a
/// short-lived access token (the standard machine-to-machine flow Google's server-side client
/// libraries wrap; this hand-rolls it so no Google SDK dependency is needed for one endpoint).
/// Node config shape: { "serviceAccountJson" - merged in by
/// GoogleSheetsAppendRowNodeExecutor from the org's stored credential - plus "spreadsheetId",
/// "sheetName" (defaults to "Sheet1") and "rowValuesJson" (a raw JSON array string, e.g.
/// ["Alice","30","Active"]) from the node itself. The service account must be shared as an
/// editor on the target spreadsheet (it has no access otherwise).</summary>
public class GoogleSheetsConnector : IConnector
{
    public string Type => "GoogleSheets";

    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string Scope = "https://www.googleapis.com/auth/spreadsheets";

    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleSheetsConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var serviceAccountJson = root.TryGetProperty("serviceAccountJson", out var saEl) ? saEl.GetString() : null;
        var spreadsheetId = root.TryGetProperty("spreadsheetId", out var idEl) ? idEl.GetString() : null;
        var sheetName = root.TryGetProperty("sheetName", out var sheetEl) ? sheetEl.GetString() : null;
        var rowValuesJson = root.TryGetProperty("rowValuesJson", out var valuesEl) ? valuesEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(serviceAccountJson))
        {
            return ConnectorResult.Fail("No Google service account is configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(spreadsheetId) || string.IsNullOrWhiteSpace(rowValuesJson))
        {
            return ConnectorResult.Fail("Google Sheets node config is missing 'spreadsheetId' or 'rowValuesJson'.");
        }

        string[] rowValues;
        try
        {
            rowValues = JsonSerializer.Deserialize<string[]>(rowValuesJson) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return ConnectorResult.Fail("Google Sheets node config's 'rowValuesJson' is not a valid JSON array.");
        }

        ServiceAccountKey serviceAccount;
        try
        {
            serviceAccount = JsonSerializer.Deserialize<ServiceAccountKey>(serviceAccountJson)
                ?? throw new JsonException("empty");
        }
        catch (JsonException)
        {
            return ConnectorResult.Fail("The configured Google service account JSON is invalid.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");

        var accessTokenResult = await FetchAccessTokenAsync(client, serviceAccount, cancellationToken);
        if (accessTokenResult.ErrorMessage is not null)
        {
            return ConnectorResult.Fail(accessTokenResult.ErrorMessage);
        }

        var range = Uri.EscapeDataString($"{(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName)}!A1");
        var appendUrl = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}:append" +
                         "?valueInputOption=USER_ENTERED&insertDataOption=INSERT_ROWS";

        var request = new HttpRequestMessage(HttpMethod.Post, appendUrl)
        {
            Content = JsonContent.Create(new { values = new[] { rowValues } }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.AccessToken);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"Google Sheets API error ({(int)response.StatusCode}): {body}");
            }

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { appended = true, response = body }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"Google Sheets request error: {ex.Message}");
        }
    }

    private static async Task<(string? AccessToken, string? ErrorMessage)> FetchAccessTokenAsync(
        HttpClient client, ServiceAccountKey serviceAccount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(serviceAccount.ClientEmail) || string.IsNullOrWhiteSpace(serviceAccount.PrivateKey))
        {
            return (null, "The configured Google service account JSON is missing 'client_email' or 'private_key'.");
        }

        var now = DateTimeOffset.UtcNow;
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
        var claims = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = serviceAccount.ClientEmail,
            scope = Scope,
            aud = TokenEndpoint,
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddHours(1).ToUnixTimeSeconds(),
        }));

        var unsignedToken = $"{header}.{claims}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(serviceAccount.PrivateKey);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var jwt = $"{unsignedToken}.{Base64UrlEncode(signature)}";

        try
        {
            var response = await client.PostAsync(TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = jwt,
            }), cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (null, $"Google token exchange failed ({(int)response.StatusCode}): {body}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body);
            return string.IsNullOrWhiteSpace(tokenResponse?.AccessToken)
                ? (null, "Google token exchange returned no access token.")
                : (tokenResponse.AccessToken, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return (null, $"Google token exchange request error: {ex.Message}");
        }
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private record ServiceAccountKey(
        [property: JsonPropertyName("client_email")] string? ClientEmail,
        [property: JsonPropertyName("private_key")] string? PrivateKey);

    private record TokenResponse([property: JsonPropertyName("access_token")] string? AccessToken);
}
