using System.Text.Json;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.ThreatIntelligence;

/// <summary>VirusTotal v3 — requires an API key. Reports Unavailable (not Error, not a
/// fabricated NoThreat) when no key is configured.</summary>
public class VirusTotalProvider : IThreatIntelligenceProvider
{
    public const string HttpClientName = "ThreatIntel:VirusTotal";
    public string ProviderSlug => "virustotal";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public VirusTotalProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ProviderCheckResult> CheckAsync(string url, string domain, CancellationToken ct = default)
    {
        var apiKey = _configuration["ThreatIntel:VirusTotal:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return ProviderCheckResult.Unavailable("No API key configured (ThreatIntel:VirusTotal:ApiKey).");

        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Add("x-apikey", apiKey);

            var urlId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            using var response = await client.GetAsync($"https://www.virustotal.com/api/v3/urls/{urlId}", ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ProviderCheckResult.NoThreat(body); // VirusTotal has never scanned this URL

            if (!response.IsSuccessStatusCode)
                return ProviderCheckResult.Error($"VirusTotal returned {(int)response.StatusCode}.");

            using var json = JsonDocument.Parse(body);
            var stats = json.RootElement.GetProperty("data").GetProperty("attributes").GetProperty("last_analysis_stats");
            var malicious = stats.GetProperty("malicious").GetInt32();
            var suspicious = stats.GetProperty("suspicious").GetInt32();

            return malicious + suspicious > 0
                ? ProviderCheckResult.Malicious(malicious > 0 ? "malicious" : "suspicious", body)
                : ProviderCheckResult.NoThreat(body);
        }
        catch (Exception ex)
        {
            return ProviderCheckResult.Error(ex.Message);
        }
    }
}
