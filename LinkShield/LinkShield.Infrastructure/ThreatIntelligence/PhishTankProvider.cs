using System.Text.Json;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.ThreatIntelligence;

/// <summary>PhishTank — documented as working without an app_key at a lower rate limit, but
/// confirmed live against the real endpoint that it now returns 403 without one. Reports
/// Unavailable when no key is configured, same as the other keyed providers.</summary>
public class PhishTankProvider : IThreatIntelligenceProvider
{
    public const string HttpClientName = "ThreatIntel:PhishTank";
    public string ProviderSlug => "phishtank";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PhishTankProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ProviderCheckResult> CheckAsync(string url, string domain, CancellationToken ct = default)
    {
        var appKey = _configuration["ThreatIntel:PhishTank:ApiKey"];
        if (string.IsNullOrWhiteSpace(appKey))
            return ProviderCheckResult.Unavailable("No API key configured (ThreatIntel:PhishTank:ApiKey) — confirmed live that PhishTank now rejects keyless requests.");

        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);

            var fields = new Dictionary<string, string> { ["url"] = url, ["format"] = "json", ["app_key"] = appKey };
            using var content = new FormUrlEncodedContent(fields);
            using var response = await client.PostAsync("https://checkurl.phishtank.com/checkurl/", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return ProviderCheckResult.Error($"PhishTank returned {(int)response.StatusCode}.");

            using var json = JsonDocument.Parse(body);
            if (!json.RootElement.TryGetProperty("results", out var results))
                return ProviderCheckResult.Error("PhishTank response missing 'results'.");

            var inDatabase = results.TryGetProperty("in_database", out var inDb) && inDb.GetBoolean();
            var valid = results.TryGetProperty("valid", out var v) && v.ValueKind == JsonValueKind.True;

            return inDatabase && valid
                ? ProviderCheckResult.Malicious("phishing", body)
                : ProviderCheckResult.NoThreat(body);
        }
        catch (Exception ex)
        {
            return ProviderCheckResult.Error(ex.Message);
        }
    }
}
