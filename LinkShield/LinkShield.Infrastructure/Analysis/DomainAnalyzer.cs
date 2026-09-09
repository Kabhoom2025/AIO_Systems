using System.Net.Http.Json;
using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.Interfaces;

namespace LinkShield.Infrastructure.Analysis;

/// <summary>Queries RDAP (rdap.org bootstrap, which redirects to the authoritative registry
/// server for the TLD) for registrar/registration-date/nameserver info. Every connection this
/// makes — including ones opened while following the bootstrap redirect — goes through the
/// SSRF-safe client's ConnectCallback.</summary>
public class DomainAnalyzer : IDomainAnalyzer
{
    public const string HttpClientName = "RdapClient";

    private readonly IHttpClientFactory _httpClientFactory;

    public DomainAnalyzer(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DomainAnalysisResultDto> AnalyzeAsync(string registeredDomain, CancellationToken ct = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            var response = await client.GetAsync($"https://rdap.org/domain/{registeredDomain}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return Failure(registeredDomain, $"RDAP lookup returned {(int)response.StatusCode} {response.StatusCode}.");
            }

            var rdap = await response.Content.ReadFromJsonAsync<RdapDomainResponse>(cancellationToken: ct);
            if (rdap is null) return Failure(registeredDomain, "RDAP response could not be parsed.");

            var registeredAtUtc = rdap.Events?.FirstOrDefault(e =>
                string.Equals(e.EventAction, "registration", StringComparison.OrdinalIgnoreCase))?.EventDate;
            var expiresAtUtc = rdap.Events?.FirstOrDefault(e =>
                string.Equals(e.EventAction, "expiration", StringComparison.OrdinalIgnoreCase))?.EventDate;

            var registrar = rdap.Entities?
                .FirstOrDefault(e => e.Roles?.Contains("registrar", StringComparer.OrdinalIgnoreCase) == true)
                is { } registrarEntity
                ? ExtractVcardName(registrarEntity)
                : null;

            var nameservers = rdap.Nameservers?
                .Where(ns => !string.IsNullOrWhiteSpace(ns.LdhName))
                .Select(ns => ns.LdhName!.TrimEnd('.'))
                .ToList() ?? [];

            var domainAgeDays = registeredAtUtc.HasValue
                ? (int)(DateTime.UtcNow - registeredAtUtc.Value).TotalDays
                : (int?)null;

            var score = 0;
            if (domainAgeDays is >= 0 and < 30) score += 25;
            else if (domainAgeDays is >= 30 and < 90) score += 10;
            if (nameservers.Count == 0) score += 10;

            return new DomainAnalysisResultDto(
                Domain: registeredDomain,
                RegisteredDomain: registeredDomain,
                Subdomain: string.Empty,
                Tld: registeredDomain.Contains('.') ? registeredDomain[(registeredDomain.LastIndexOf('.') + 1)..] : string.Empty,
                Registrar: registrar,
                RegisteredAtUtc: registeredAtUtc,
                ExpiresAtUtc: expiresAtUtc,
                DomainAgeDays: domainAgeDays,
                Organization: null,
                Country: null,
                Nameservers: nameservers,
                RdapLookupSucceeded: true,
                RdapError: null,
                DomainAnalysisScore: Math.Min(score, 100));
        }
        catch (Exception ex)
        {
            return Failure(registeredDomain, ex.Message);
        }
    }

    private static string? ExtractVcardName(RdapEntity entity)
    {
        if (entity.VcardArray is not { } vcard || vcard.ValueKind != System.Text.Json.JsonValueKind.Array || vcard.GetArrayLength() < 2)
            return null;

        foreach (var field in vcard[1].EnumerateArray())
        {
            if (field.ValueKind != System.Text.Json.JsonValueKind.Array || field.GetArrayLength() < 4) continue;
            if (field[0].GetString() == "fn")
                return field[3].GetString();
        }
        return null;
    }

    private static DomainAnalysisResultDto Failure(string registeredDomain, string error) => new(
        Domain: registeredDomain,
        RegisteredDomain: registeredDomain,
        Subdomain: string.Empty,
        Tld: registeredDomain.Contains('.') ? registeredDomain[(registeredDomain.LastIndexOf('.') + 1)..] : string.Empty,
        Registrar: null,
        RegisteredAtUtc: null,
        ExpiresAtUtc: null,
        DomainAgeDays: null,
        Organization: null,
        Country: null,
        Nameservers: [],
        RdapLookupSucceeded: false,
        RdapError: error,
        DomainAnalysisScore: 0);
}
