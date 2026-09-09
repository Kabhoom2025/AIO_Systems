using System.Text;
using System.Text.Json;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.ThreatIntelligence;

/// <summary>Google Safe Browsing v4 — requires an API key. Reports Unavailable (not Error,
/// not a fabricated NoThreat) when no key is configured, so the gap in coverage is visible
/// rather than silently masked.</summary>
public class GoogleSafeBrowsingProvider : IThreatIntelligenceProvider
{
    public const string HttpClientName = "ThreatIntel:GoogleSafeBrowsing";
    public string ProviderSlug => "google-safe-browsing";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GoogleSafeBrowsingProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ProviderCheckResult> CheckAsync(string url, string domain, CancellationToken ct = default)
    {
        var apiKey = _configuration["ThreatIntel:GoogleSafeBrowsing:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return ProviderCheckResult.Unavailable("No API key configured (ThreatIntel:GoogleSafeBrowsing:ApiKey).");

        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            var requestBody = JsonSerializer.Serialize(new
            {
                client = new { clientId = "linkshield-ai", clientVersion = "1.0.0" },
                threatInfo = new
                {
                    threatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE" },
                    platformTypes = new[] { "ANY_PLATFORM" },
                    threatEntryTypes = new[] { "URL" },
                    threatEntries = new[] { new { url } }
                }
            });

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(
                $"https://safebrowsing.googleapis.com/v4/threatMatches:find?key={apiKey}", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return ProviderCheckResult.Error($"Google Safe Browsing returned {(int)response.StatusCode}.");

            using var json = JsonDocument.Parse(body);
            if (!json.RootElement.TryGetProperty("matches", out var matches) || matches.GetArrayLength() == 0)
                return ProviderCheckResult.NoThreat(body);

            var category = matches[0].TryGetProperty("threatType", out var tt) ? tt.GetString() ?? "unknown" : "unknown";
            return ProviderCheckResult.Malicious(category, body);
        }
        catch (Exception ex)
        {
            return ProviderCheckResult.Error(ex.Message);
        }
    }
}
