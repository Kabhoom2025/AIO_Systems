using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LinkShield.Infrastructure.Risk;

public class MlPredictionClient : IMlPredictionClient
{
    public const string HttpClientName = "MlServiceClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MlPredictionClient> _logger;

    public MlPredictionClient(IHttpClientFactory httpClientFactory, ILogger<MlPredictionClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MlPredictionResultDto?> PredictAsync(Guid scanId, MlFeaturesDto features, CancellationToken ct = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            var request = new PredictRequest(scanId.ToString(), new UrlFeaturesPayload(
                features.UrlLength, features.DomainLength, features.PathLength, features.QueryLength,
                features.SubdomainCount, features.DotCount, features.HyphenCount, features.DigitCount, features.SpecialCharCount,
                features.IsIpAddressHost, features.IsHttps, features.HasUrlEncoding, features.HasPunycode,
                features.IsShortenedUrl, features.HasSuspiciousTld, features.HasRedirectParameter,
                features.DomainAgeDays, features.RedirectCount, features.HasMailConfiguration,
                features.SslIsValid, features.SslDomainMatches));

            using var response = await client.PostAsJsonAsync("/api/v1/predict", request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogInformation("ml-service has no trained model yet — skipping ML signal for scan {ScanId}", scanId);
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ml-service returned {StatusCode} for scan {ScanId}", response.StatusCode, scanId);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<PredictResponse>(cancellationToken: ct);
            return body is null ? null : new MlPredictionResultDto(body.Prediction, body.Probability, body.ModelVersion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not reach ml-service for scan {ScanId}", scanId);
            return null;
        }
    }

    private record PredictRequest(
        [property: JsonPropertyName("scan_id")] string ScanId,
        [property: JsonPropertyName("features")] UrlFeaturesPayload Features);

    private record UrlFeaturesPayload(
        [property: JsonPropertyName("url_length")] int UrlLength,
        [property: JsonPropertyName("domain_length")] int DomainLength,
        [property: JsonPropertyName("path_length")] int PathLength,
        [property: JsonPropertyName("query_length")] int QueryLength,
        [property: JsonPropertyName("subdomain_count")] int SubdomainCount,
        [property: JsonPropertyName("dot_count")] int DotCount,
        [property: JsonPropertyName("hyphen_count")] int HyphenCount,
        [property: JsonPropertyName("digit_count")] int DigitCount,
        [property: JsonPropertyName("special_char_count")] int SpecialCharCount,
        [property: JsonPropertyName("is_ip_address_host")] bool IsIpAddressHost,
        [property: JsonPropertyName("is_https")] bool IsHttps,
        [property: JsonPropertyName("has_url_encoding")] bool HasUrlEncoding,
        [property: JsonPropertyName("has_punycode")] bool HasPunycode,
        [property: JsonPropertyName("is_shortened_url")] bool IsShortenedUrl,
        [property: JsonPropertyName("has_suspicious_tld")] bool HasSuspiciousTld,
        [property: JsonPropertyName("has_redirect_parameter")] bool HasRedirectParameter,
        [property: JsonPropertyName("domain_age_days")] int? DomainAgeDays,
        [property: JsonPropertyName("redirect_count")] int RedirectCount,
        [property: JsonPropertyName("has_mail_configuration")] bool HasMailConfiguration,
        [property: JsonPropertyName("ssl_is_valid")] bool SslIsValid,
        [property: JsonPropertyName("ssl_domain_matches")] bool SslDomainMatches);

    private record PredictResponse(
        [property: JsonPropertyName("prediction")] string Prediction,
        [property: JsonPropertyName("probability")] decimal Probability,
        [property: JsonPropertyName("model_version")] string ModelVersion);
}
