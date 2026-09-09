using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Risk;

/// <summary>
/// Combines every stage's score into one weighted 0-100 risk score with a full explanation
/// trail. Weights and thresholds are read from SystemSettings (seeded with the spec section 12
/// defaults) rather than hardcoded, so they're admin-tunable without a redeploy. The individual
/// RiskRule rows are seeded reference data describing the intended rules but are not yet
/// evaluated by an expression engine — this implementation is code-driven per category. Wiring
/// RiskRule.ConditionExpression through an actual evaluator is a documented follow-up, not
/// silently dropped scope.
/// </summary>
public class RiskEngine : IRiskEngine
{
    private readonly LinkShieldDbContext _db;

    public RiskEngine(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<RiskEngineResultDto> ComputeAsync(RiskEngineInput input, CancellationToken ct = default)
    {
        var settings = await _db.SystemSettings
            .Where(s => s.Category == "RiskEngine")
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        double Weight(string key, double fallback) =>
            settings.TryGetValue($"RiskEngine:Weight:{key}", out var v) && double.TryParse(v, out var d) ? d : fallback;

        int Threshold(string key, int fallback) =>
            settings.TryGetValue($"RiskEngine:{key}Threshold", out var v) && int.TryParse(v, out var i) ? i : fallback;

        var factors = new List<RiskFactorDto>();
        var signalsAvailable = 0;
        var signalsTotal = 8;

        // URL analysis — always available (pure computation).
        signalsAvailable++;
        var urlScore = input.UrlAnalysis.UrlAnalysisScore;
        if (input.UrlAnalysis.SuspiciousKeywords.Count > 0)
            factors.Add(new RiskFactorDto(RiskFactorCategory.UrlAnalysis,
                $"Contains suspicious keyword(s): {string.Join(", ", input.UrlAnalysis.SuspiciousKeywords)}", 6));
        if (input.UrlAnalysis.IsIpAddressHost)
            factors.Add(new RiskFactorDto(RiskFactorCategory.UrlAnalysis, "Uses an IP address instead of a hostname", 12));
        if (input.UrlAnalysis.HasPunycode)
            factors.Add(new RiskFactorDto(RiskFactorCategory.UrlAnalysis, "Hostname uses Punycode encoding", 10));
        if (input.UrlAnalysis.HasSuspiciousTld)
            factors.Add(new RiskFactorDto(RiskFactorCategory.UrlAnalysis, "Uses a commonly-abused top-level domain", 10));

        // Domain intelligence.
        var domainScore = 0;
        if (input.DomainAnalysis is { } domain)
        {
            signalsAvailable++;
            domainScore = domain.DomainAnalysisScore;
            if (domain.DomainAgeDays is { } age && age < 30)
                factors.Add(new RiskFactorDto(RiskFactorCategory.DomainIntelligence, $"Domain registered {age} day(s) ago", 10));
            else if (!domain.RdapLookupSucceeded)
                factors.Add(new RiskFactorDto(RiskFactorCategory.DomainIntelligence, $"RDAP lookup failed: {domain.RdapError}", 0));
        }

        // DNS.
        var dnsScore = 0;
        if (input.DnsAnalysis is { } dns)
        {
            signalsAvailable++;
            dnsScore = dns.DnsAnalysisScore;
            if (dns.HasSuspiciousNameservers)
                factors.Add(new RiskFactorDto(RiskFactorCategory.DnsAnalysis, "Uses nameservers from a commonly-abused free DNS provider", 15));
            if (!dns.HasMailConfiguration)
                factors.Add(new RiskFactorDto(RiskFactorCategory.DnsAnalysis, "No mail (MX) configuration found", 3));
        }

        // SSL/TLS.
        var sslScore = 0;
        if (input.SslAnalysis is { } ssl)
        {
            signalsAvailable++;
            sslScore = ssl.SslAnalysisScore;
            if (ssl.IsExpired) factors.Add(new RiskFactorDto(RiskFactorCategory.SslAnalysis, "TLS certificate is expired", 15));
            if (ssl.IsSelfSigned) factors.Add(new RiskFactorDto(RiskFactorCategory.SslAnalysis, "TLS certificate is self-signed", 12));
            if (ssl.HasCertificate && !ssl.DomainMatchesCertificate)
                factors.Add(new RiskFactorDto(RiskFactorCategory.SslAnalysis, "TLS certificate does not match the hostname", 12));
        }

        // Redirects.
        var redirectScore = 0;
        if (input.RedirectAnalysis is { } redirect)
        {
            signalsAvailable++;
            redirectScore = redirect.RedirectAnalysisScore;
            if (redirect.RedirectCount > 2)
                factors.Add(new RiskFactorDto(RiskFactorCategory.RedirectAnalysis,
                    $"{redirect.RedirectCount} redirects, {(redirect.AnyDomainChange ? "including a domain change" : "no domain change")}", 8));
            if (redirect.AnyHttpsDowngrade)
                factors.Add(new RiskFactorDto(RiskFactorCategory.RedirectAnalysis, "A redirect downgrades from HTTPS to HTTP", 10));
        }

        // Threat intelligence.
        signalsAvailable++;
        var confirmedMalicious = input.ThreatIntelligenceResults.Where(r => r.Status == ThreatIntelligenceStatus.ConfirmedMalicious).ToList();
        var tiScore = confirmedMalicious.Count > 0 ? 100
            : input.ThreatIntelligenceResults.Any(r => r.Status == ThreatIntelligenceStatus.Suspicious) ? 50 : 0;
        if (confirmedMalicious.Count > 0)
            factors.Add(new RiskFactorDto(RiskFactorCategory.ThreatIntelligence,
                $"Confirmed malicious by {string.Join(", ", confirmedMalicious.Select(r => r.ProviderName))}", 40));

        // Brand impersonation.
        signalsAvailable++;
        var brandScore = 0;
        foreach (var match in input.BrandMatches)
        {
            var matchScore = match.MatchType switch { "Homoglyph" => 90, "Typosquat" => 70, _ => 60 };
            brandScore = Math.Max(brandScore, matchScore);
            factors.Add(new RiskFactorDto(RiskFactorCategory.BrandImpersonation,
                $"{match.MatchType} of {match.OfficialDomain} (Levenshtein {match.LevenshteinDistance:0}, Jaro-Winkler {match.JaroWinklerSimilarity:0.00})", 20));
        }

        // ML.
        var mlScore = 0;
        if (input.MlPrediction is { } ml)
        {
            signalsAvailable++;
            mlScore = (int)Math.Round(ml.Probability * 100);
            if (ml.Prediction == "phishing")
                factors.Add(new RiskFactorDto(RiskFactorCategory.MachineLearning,
                    $"ML model predicts phishing with {ml.Probability:P0} probability (model {ml.ModelVersion})", 5));
        }

        var weightedScore =
            urlScore * Weight("UrlAnalysis", 0.15) +
            domainScore * Weight("DomainIntelligence", 0.15) +
            dnsScore * Weight("DnsAnalysis", 0.05) +
            sslScore * Weight("SslAnalysis", 0.05) +
            redirectScore * Weight("RedirectAnalysis", 0.10) +
            tiScore * Weight("ThreatIntelligence", 0.30) +
            brandScore * Weight("BrandDetection", 0.15) +
            mlScore * Weight("MachineLearning", 0.05);

        var riskScore = Math.Clamp((int)Math.Round(weightedScore), 0, 100);

        var lowThreshold = Threshold("Low", 20);
        var moderateThreshold = Threshold("Moderate", 40);
        var highThreshold = Threshold("High", 70);

        var riskLevel = riskScore switch
        {
            var s when s <= lowThreshold => RiskLevel.Low,
            var s when s <= moderateThreshold => RiskLevel.Moderate,
            var s when s <= highThreshold => RiskLevel.High,
            _ => RiskLevel.Critical
        };

        // Only a confirmed threat-intelligence hit ever produces Malicious — scoring alone
        // (however high) tops out at Suspicious, per the spec's no-false-confirmation rule.
        ScanVerdict verdict;
        if (confirmedMalicious.Count > 0) verdict = ScanVerdict.Malicious;
        else if (riskScore > moderateThreshold) verdict = ScanVerdict.Suspicious;
        else if (riskScore > lowThreshold) verdict = ScanVerdict.LowRisk;
        else if (signalsAvailable == signalsTotal && riskScore == 0 && brandScore == 0) verdict = ScanVerdict.Safe;
        else verdict = signalsAvailable < signalsTotal ? ScanVerdict.Unknown : ScanVerdict.LowRisk;

        var confidence = Math.Round((decimal)signalsAvailable / signalsTotal, 2);

        return new RiskEngineResultDto(riskScore, riskLevel, verdict, confidence, factors);
    }
}
