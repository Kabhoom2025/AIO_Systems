using System.Text.Json;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.ThreatIntelligence;

/// <summary>URLhaus (abuse.ch) — as of abuse.ch's 2024/2025 policy change, their API now
/// requires an Auth-Key header for all requests (previously free/keyless); confirmed live
/// against the real endpoint, which returns 401 without one. Reports Unavailable when no key
/// is configured, same as the other keyed providers.</summary>
public class UrlhausProvider : IThreatIntelligenceProvider
{
    public const string HttpClientName = "ThreatIntel:URLhaus";
    public string ProviderSlug => "urlhaus";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public UrlhausProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ProviderCheckResult> CheckAsync(string url, string domain, CancellationToken ct = default)
    {
        var apiKey = _configuration["ThreatIntel:URLhaus:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return ProviderCheckResult.Unavailable("No API key configured (ThreatIntel:URLhaus:ApiKey) — abuse.ch requires an Auth-Key header as of their 2024/2025 policy change.");

        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Add("Auth-Key", apiKey);
            using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["url"] = url });
            using var response = await client.PostAsync("https://urlhaus-api.abuse.ch/v1/url/", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return ProviderCheckResult.Error($"URLhaus returned {(int)response.StatusCode}.");

            using var json = JsonDocument.Parse(body);
            var queryStatus = json.RootElement.TryGetProperty("query_status", out var qs) ? qs.GetString() : null;

            return queryStatus switch
            {
                "ok" => ProviderCheckResult.Malicious(
                    json.RootElement.TryGetProperty("threat", out var threat) ? threat.GetString() ?? "malware_download" : "malware_download",
                    body),
                "no_results" => ProviderCheckResult.NoThreat(body),
                _ => ProviderCheckResult.Error($"URLhaus query_status='{queryStatus}'.")
            };
        }
        catch (Exception ex)
        {
            return ProviderCheckResult.Error(ex.Message);
        }
    }
}
