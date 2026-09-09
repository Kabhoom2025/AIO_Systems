using System.Text.Json;
using LinkShield.Application.Common;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkShield.Infrastructure.BackgroundProcessing;

public class ScanPipelineRunner : IScanPipelineRunner
{
    private readonly LinkShieldDbContext _db;
    private readonly IUrlAnalyzer _urlAnalyzer;
    private readonly IDomainAnalyzer _domainAnalyzer;
    private readonly IDnsAnalyzer _dnsAnalyzer;
    private readonly ISslAnalyzer _sslAnalyzer;
    private readonly IRedirectAnalyzer _redirectAnalyzer;
    private readonly IThreatIntelligenceService _threatIntelligenceService;
    private readonly IBrandDetectionService _brandDetectionService;
    private readonly IMlPredictionClient _mlPredictionClient;
    private readonly IRiskEngine _riskEngine;
    private readonly IScanNotifier _notifier;
    private readonly ILogger<ScanPipelineRunner> _logger;

    public ScanPipelineRunner(
        LinkShieldDbContext db, IUrlAnalyzer urlAnalyzer, IDomainAnalyzer domainAnalyzer, IDnsAnalyzer dnsAnalyzer,
        ISslAnalyzer sslAnalyzer, IRedirectAnalyzer redirectAnalyzer, IThreatIntelligenceService threatIntelligenceService,
        IBrandDetectionService brandDetectionService, IMlPredictionClient mlPredictionClient, IRiskEngine riskEngine,
        IScanNotifier notifier, ILogger<ScanPipelineRunner> logger)
    {
        _db = db;
        _urlAnalyzer = urlAnalyzer;
        _domainAnalyzer = domainAnalyzer;
        _dnsAnalyzer = dnsAnalyzer;
        _sslAnalyzer = sslAnalyzer;
        _redirectAnalyzer = redirectAnalyzer;
        _threatIntelligenceService = threatIntelligenceService;
        _brandDetectionService = brandDetectionService;
        _mlPredictionClient = mlPredictionClient;
        _riskEngine = riskEngine;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task RunAsync(Guid scanId, CancellationToken ct = default)
    {
        var scan = await _db.UrlScans.FirstOrDefaultAsync(s => s.Id == scanId, ct);
        if (scan is null || scan.Status is ScanStatus.Completed or ScanStatus.Failed) return;

        try
        {
            var uri = new Uri(scan.NormalizedUrl);
            var registeredDomain = DomainNameHelper.GetRegisteredDomain(uri.Host);

            var domainResult = await RunStageAsync(scan, ScanStatus.DomainAnalysis, ct, async () =>
            {
                var result = await _domainAnalyzer.AnalyzeAsync(registeredDomain, ct);
                _db.DomainAnalyses.Add(new DomainAnalysis
                {
                    UrlScanId = scan.Id, Domain = uri.Host, RegisteredDomain = result.RegisteredDomain,
                    Subdomain = uri.Host == registeredDomain ? string.Empty : uri.Host[..^(registeredDomain.Length + 1)],
                    Tld = result.Tld, Registrar = result.Registrar, RegisteredAtUtc = result.RegisteredAtUtc,
                    ExpiresAtUtc = result.ExpiresAtUtc, DomainAgeDays = result.DomainAgeDays, Organization = result.Organization,
                    Country = result.Country, NameserversJson = JsonSerializer.Serialize(result.Nameservers),
                    RdapLookupSucceeded = result.RdapLookupSucceeded, RdapError = result.RdapError,
                    DomainAnalysisScore = result.DomainAnalysisScore
                });
                return result;
            });

            var dnsResult = await RunStageAsync(scan, ScanStatus.DnsAnalysis, ct, async () =>
            {
                var result = await _dnsAnalyzer.AnalyzeAsync(uri.Host, ct);
                _db.DnsAnalyses.Add(new DnsAnalysis
                {
                    UrlScanId = scan.Id, Resolves = result.Resolves,
                    ARecordsJson = JsonSerializer.Serialize(result.ARecords), AaaaRecordsJson = JsonSerializer.Serialize(result.AaaaRecords),
                    MxRecordsJson = JsonSerializer.Serialize(result.MxRecords), NsRecordsJson = JsonSerializer.Serialize(result.NsRecords),
                    CnameRecordsJson = JsonSerializer.Serialize(result.CnameRecords), TxtRecordsJson = JsonSerializer.Serialize(result.TxtRecords),
                    HasMailConfiguration = result.HasMailConfiguration, HasSuspiciousNameservers = result.HasSuspiciousNameservers,
                    LookupError = result.LookupError, DnsAnalysisScore = result.DnsAnalysisScore
                });
                return result;
            });

            var sslResult = await RunStageAsync(scan, ScanStatus.SslAnalysis, ct, async () =>
            {
                var result = await _sslAnalyzer.AnalyzeAsync(uri.Host, ct);
                _db.SslAnalyses.Add(new SslAnalysis
                {
                    UrlScanId = scan.Id, HasCertificate = result.HasCertificate, IsValid = result.IsValid,
                    Issuer = result.Issuer, Subject = result.Subject,
                    SubjectAlternativeNamesJson = JsonSerializer.Serialize(result.SubjectAlternativeNames),
                    ValidFromUtc = result.ValidFromUtc, ValidToUtc = result.ValidToUtc, IsExpired = result.IsExpired,
                    IsSelfSigned = result.IsSelfSigned, DomainMatchesCertificate = result.DomainMatchesCertificate,
                    TlsVersion = result.TlsVersion, ChainValidationError = result.ChainValidationError,
                    SslAnalysisScore = result.SslAnalysisScore
                });
                return result;
            });

            var redirectResult = await RunStageAsync(scan, ScanStatus.RedirectAnalysis, ct, async () =>
            {
                var result = await _redirectAnalyzer.AnalyzeAsync(scan.NormalizedUrl, ct);
                foreach (var hop in result.Hops)
                {
                    _db.RedirectAnalyses.Add(new RedirectAnalysis
                    {
                        UrlScanId = scan.Id, HopIndex = hop.HopIndex, FromUrl = hop.FromUrl, ToUrl = hop.ToUrl,
                        StatusCode = hop.StatusCode, DomainChanged = hop.DomainChanged, HttpsDowngrade = hop.HttpsDowngrade,
                        TargetIsSuspicious = hop.TargetIsSuspicious
                    });
                }
                return result;
            });

            var tiResults = await RunStageAsync(scan, ScanStatus.ThreatIntelligence, ct, () =>
                _threatIntelligenceService.CheckAllAsync(scan.Id, scan.NormalizedUrl, registeredDomain, ct));

            var brandMatches = await RunStageAsync(scan, ScanStatus.BrandDetection, ct, async () =>
            {
                var matches = await _brandDetectionService.DetectAsync(registeredDomain, scan.NormalizedUrl, ct);
                foreach (var match in matches)
                {
                    _db.BrandMatches.Add(new BrandMatch
                    {
                        UrlScanId = scan.Id, BrandProfileId = match.BrandProfileId, MatchedDomain = match.MatchedDomain,
                        MatchType = match.MatchType, LevenshteinDistance = match.LevenshteinDistance,
                        JaroWinklerSimilarity = match.JaroWinklerSimilarity, ContainsAuthKeywords = match.ContainsAuthKeywords
                    });
                }
                return matches;
            });

            var urlFeatures = _urlAnalyzer.Analyze(scan.OriginalUrl);
            var mlPrediction = await RunStageAsync(scan, ScanStatus.MlAnalysis, ct, async () =>
            {
                var features = new MlFeaturesDto(
                    urlFeatures.UrlLength, urlFeatures.DomainLength, urlFeatures.PathLength, urlFeatures.QueryLength,
                    urlFeatures.SubdomainCount, urlFeatures.DotCount, urlFeatures.HyphenCount, urlFeatures.DigitCount,
                    urlFeatures.SpecialCharCount, urlFeatures.IsIpAddressHost, urlFeatures.IsHttps, urlFeatures.HasUrlEncoding,
                    urlFeatures.HasPunycode, urlFeatures.IsShortenedUrl, urlFeatures.HasSuspiciousTld, urlFeatures.HasRedirectParameter,
                    domainResult.DomainAgeDays, redirectResult.RedirectCount, dnsResult.HasMailConfiguration,
                    sslResult.IsValid, sslResult.DomainMatchesCertificate);

                var prediction = await _mlPredictionClient.PredictAsync(scan.Id, features, ct);
                _db.MlPredictions.Add(new MlPrediction
                {
                    UrlScanId = scan.Id,
                    Prediction = prediction?.Prediction ?? string.Empty,
                    Probability = prediction?.Probability ?? 0m,
                    ModelVersion = prediction?.ModelVersion ?? string.Empty,
                    FeaturesJson = JsonSerializer.Serialize(features),
                    RequestSucceeded = prediction is not null,
                    ErrorMessage = prediction is null ? "ml-service unavailable or no trained model." : null
                });
                return prediction;
            });

            await UpdateStatusAsync(scan, ScanStatus.RiskScoring, "Computing final risk score.", ct);
            var riskResult = await _riskEngine.ComputeAsync(new RiskEngineInput(
                urlFeatures, domainResult, dnsResult, sslResult, redirectResult, tiResults, brandMatches, mlPrediction), ct);

            foreach (var factor in riskResult.Factors)
            {
                _db.RiskFactors.Add(new RiskFactor
                {
                    UrlScanId = scan.Id, Category = factor.Category, Description = factor.Description,
                    ScoreContribution = factor.ScoreContribution
                });
            }

            scan.RiskScore = riskResult.RiskScore;
            scan.RiskLevel = riskResult.RiskLevel;
            scan.Verdict = riskResult.Verdict;
            scan.Confidence = riskResult.Confidence;
            scan.Status = ScanStatus.Completed;
            scan.CompletedAtUtc = DateTime.UtcNow;
            _db.ScanEvents.Add(new ScanEvent { UrlScanId = scan.Id, Stage = ScanStatus.Completed, Message = "Scan completed." });
            await _db.SaveChangesAsync(ct);
            await _notifier.NotifyStageAsync(scan.Id, ScanStatus.Completed, "Scan completed.", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scan pipeline failed for scan {ScanId}", scanId);
            scan.Status = ScanStatus.Failed;
            scan.FailureReason = ex.Message;
            _db.ScanEvents.Add(new ScanEvent { UrlScanId = scan.Id, Stage = ScanStatus.Failed, Message = ex.Message });
            await _db.SaveChangesAsync(ct);
            await _notifier.NotifyStageAsync(scan.Id, ScanStatus.Failed, ex.Message, ct);
        }
    }

    private async Task<T> RunStageAsync<T>(UrlScan scan, ScanStatus stage, CancellationToken ct, Func<Task<T>> action)
    {
        await UpdateStatusAsync(scan, stage, $"{stage} started.", ct);
        var result = await action();
        await _db.SaveChangesAsync(ct);
        return result;
    }

    private async Task UpdateStatusAsync(UrlScan scan, ScanStatus stage, string message, CancellationToken ct)
    {
        scan.Status = stage;
        _db.ScanEvents.Add(new ScanEvent { UrlScanId = scan.Id, Stage = stage, Message = message });
        await _db.SaveChangesAsync(ct);
        await _notifier.NotifyStageAsync(scan.Id, stage, message, ct);
    }
}
