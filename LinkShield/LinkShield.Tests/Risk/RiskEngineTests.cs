using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using LinkShield.Infrastructure.Risk;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkShield.Tests.Risk;

public class RiskEngineTests
{
    private static LinkShieldDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LinkShieldDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LinkShieldDbContext(options);
    }

    private static UrlAnalysisResultDto CleanUrlAnalysis() => new(
        "https://example.com/", "https://example.com/", 20, 11, 1, 0, 0, 1, 0, 0, 0,
        false, true, false, false, false, false, false, false, false, false, [], [], 0);

    private static DomainAnalysisResultDto CleanDomain() => new(
        "example.com", "example.com", "", "com", "Example Registrar",
        DateTime.UtcNow.AddYears(-5), DateTime.UtcNow.AddYears(1), 365 * 5,
        null, null, ["ns1.example.com", "ns2.example.com"], true, null, 0);

    private static DnsAnalysisResultDto CleanDns() => new(
        true, ["93.184.216.34"], [], ["mail.example.com"], ["ns1.example.com"], [], [], true, false, null, 0);

    private static SslAnalysisResultDto CleanSsl() => new(
        true, true, "CN=DigiCert", "CN=example.com", ["example.com"],
        DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(11), false, false, true, "Tls13", null, 0);

    private static RedirectAnalysisResultDto CleanRedirect() => new(
        "https://example.com/", 0, false, false, [], null, 0);

    [Fact]
    public async Task ComputeAsync_AllCleanSignals_ProducesSafeVerdictAndZeroScore()
    {
        var engine = new RiskEngine(CreateContext());
        var input = new RiskEngineInput(
            CleanUrlAnalysis(), CleanDomain(), CleanDns(), CleanSsl(), CleanRedirect(),
            [], [], new MlPredictionResultDto("benign", 0.01m, "test-v1"));

        var result = await engine.ComputeAsync(input);

        Assert.Equal(0, result.RiskScore);
        Assert.Equal(RiskLevel.Low, result.RiskLevel);
        Assert.Equal(ScanVerdict.Safe, result.Verdict);
        Assert.Equal(1.0m, result.Confidence);
    }

    [Fact]
    public async Task ComputeAsync_ConfirmedThreatIntelHit_AlwaysProducesMalicious()
    {
        var engine = new RiskEngine(CreateContext());
        var tiResults = new List<ThreatIntelligenceResultDto>
        {
            new("PhishTank", "phishtank", ThreatIntelligenceStatus.ConfirmedMalicious, "phishing", null, false, DateTime.UtcNow)
        };
        var input = new RiskEngineInput(CleanUrlAnalysis(), CleanDomain(), CleanDns(), CleanSsl(), CleanRedirect(), tiResults, [], null);

        var result = await engine.ComputeAsync(input);

        Assert.Equal(ScanVerdict.Malicious, result.Verdict);
        Assert.Contains(result.Factors, f => f.Category == RiskFactorCategory.ThreatIntelligence);
    }

    [Fact]
    public async Task ComputeAsync_HighScoringSignalsWithoutConfirmedHit_NeverReachesMalicious()
    {
        var engine = new RiskEngine(CreateContext());
        var suspiciousUrl = CleanUrlAnalysis() with
        {
            IsIpAddressHost = true,
            HasPunycode = true,
            HasSuspiciousTld = true,
            HasSuspiciousFileExtension = true,
            IsHttps = false,
            UrlAnalysisScore = 90
        };
        var brandMatches = new List<BrandMatchResultDto>
        {
            new(Guid.NewGuid(), "PayPal", "paypal.com", "paypa1.com", "Typosquat", 1, 0.95, true)
        };
        var input = new RiskEngineInput(suspiciousUrl, CleanDomain(), CleanDns(), CleanSsl(), CleanRedirect(), [], brandMatches, null);

        var result = await engine.ComputeAsync(input);

        Assert.NotEqual(ScanVerdict.Malicious, result.Verdict);
    }

    [Fact]
    public async Task ComputeAsync_MissingStages_ReducesConfidenceAndReportsUnknown()
    {
        var engine = new RiskEngine(CreateContext());
        var input = new RiskEngineInput(CleanUrlAnalysis(), null, null, null, null, [], [], null);

        var result = await engine.ComputeAsync(input);

        Assert.True(result.Confidence < 1.0m);
        Assert.Equal(ScanVerdict.Unknown, result.Verdict);
    }
}
