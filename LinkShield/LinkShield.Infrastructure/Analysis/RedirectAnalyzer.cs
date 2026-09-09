using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LinkShield.Infrastructure.Analysis;

/// <summary>Follows redirects one hop at a time (AllowAutoRedirect=false) so every target can
/// be SSRF-validated and recorded before being followed — never lets the framework silently
/// chase a redirect into a private address. Bounded by SsrfProtection:MaxRedirects.</summary>
public class RedirectAnalyzer : IRedirectAnalyzer
{
    public const string HttpClientName = "RedirectAnalysisClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISsrfGuard _ssrfGuard;
    private readonly IUrlAnalyzer _urlAnalyzer;
    private readonly int _maxRedirects;

    public RedirectAnalyzer(
        IHttpClientFactory httpClientFactory, ISsrfGuard ssrfGuard, IUrlAnalyzer urlAnalyzer, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _ssrfGuard = ssrfGuard;
        _urlAnalyzer = urlAnalyzer;
        _maxRedirects = int.TryParse(configuration["SsrfProtection:MaxRedirects"], out var m) ? m : 5;
    }

    public async Task<RedirectAnalysisResultDto> AnalyzeAsync(string startUrl, CancellationToken ct = default)
    {
        using var client = _httpClientFactory.CreateClient(HttpClientName);
        var hops = new List<RedirectHopDto>();
        var currentUrl = startUrl;

        for (var i = 0; i < _maxRedirects; i++)
        {
            var currentUri = new Uri(currentUrl);
            var hostCheck = await _ssrfGuard.ValidateHostAsync(currentUri.Host, ct);
            if (!hostCheck.IsAllowed)
                return Result(hops, currentUrl, $"Blocked: {hostCheck.Reason}");

            HttpResponseMessage response;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, currentUri);
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            }
            catch (Exception ex)
            {
                return Result(hops, currentUrl, $"Request to '{currentUrl}' failed: {ex.Message}");
            }

            using (response)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode is < 300 or >= 400 || response.Headers.Location is null)
                    return Result(hops, currentUrl, error: null); // final destination reached

                var toUri = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(currentUri, response.Headers.Location);
                var toUrl = toUri.ToString();

                var domainChanged = !string.Equals(currentUri.Host, toUri.Host, StringComparison.OrdinalIgnoreCase);
                var httpsDowngrade = currentUri.Scheme == "https" && toUri.Scheme == "http";
                var targetSignals = _urlAnalyzer.Analyze(toUrl);
                var targetIsSuspicious = targetSignals.IsIpAddressHost || targetSignals.HasSuspiciousTld || targetSignals.IsShortenedUrl;

                hops.Add(new RedirectHopDto(i, currentUrl, toUrl, statusCode, domainChanged, httpsDowngrade, targetIsSuspicious));
                currentUrl = toUrl;
            }
        }

        return Result(hops, currentUrl, error: $"Redirect limit ({_maxRedirects}) reached without resolving to a final URL.");
    }

    private static RedirectAnalysisResultDto Result(List<RedirectHopDto> hops, string finalUrl, string? error)
    {
        var anyDomainChange = hops.Any(h => h.DomainChanged);
        var anyHttpsDowngrade = hops.Any(h => h.HttpsDowngrade);
        var anySuspiciousTarget = hops.Any(h => h.TargetIsSuspicious);

        var score = 0;
        score += Math.Min(hops.Count(h => h.DomainChanged) * 5, 20);
        if (anyHttpsDowngrade) score += 10;
        if (hops.Count > 2) score += (hops.Count - 2) * 5;
        if (anySuspiciousTarget) score += 15;

        return new RedirectAnalysisResultDto(
            FinalUrl: finalUrl,
            RedirectCount: hops.Count,
            AnyDomainChange: anyDomainChange,
            AnyHttpsDowngrade: anyHttpsDowngrade,
            Hops: hops,
            Error: error,
            RedirectAnalysisScore: Math.Min(score, 100));
    }
}
