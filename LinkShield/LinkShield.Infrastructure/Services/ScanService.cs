using System.Text.Json;
using LinkShield.Application.DTOs.Analysis;
using LinkShield.Application.DTOs.Risk;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.DTOs.ThreatIntelligence;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Entities;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

/// <summary>
/// Handles scan creation/retrieval/listing/deletion. Submitting a scan runs the URL-analysis
/// stage synchronously (fast, pure computation) and then hands the scan to IScanQueue for
/// IScanPipelineRunner to process every remaining stage (domain/DNS/SSL/redirect/threat-intel/
/// brand/ML/risk-scoring) in the background — see docs/architecture.md's core scan flow.
/// </summary>
public class ScanService : IScanService
{
    private readonly LinkShieldDbContext _db;
    private readonly IUrlAnalyzer _urlAnalyzer;
    private readonly IScanQueue _scanQueue;

    public ScanService(LinkShieldDbContext db, IUrlAnalyzer urlAnalyzer, IScanQueue scanQueue)
    {
        _db = db;
        _urlAnalyzer = urlAnalyzer;
        _scanQueue = scanQueue;
    }

    public async Task<ScanDetailDto> SubmitScanAsync(string rawUrl, Guid? userId, Guid? apiClientId = null, CancellationToken ct = default)
    {
        var analysis = _urlAnalyzer.Analyze(rawUrl);

        var scan = new UrlScan
        {
            UserId = userId,
            ApiClientId = apiClientId,
            OriginalUrl = rawUrl,
            NormalizedUrl = analysis.NormalizedUrl,
            Status = ScanStatus.UrlAnalysis,
            StartedAtUtc = DateTime.UtcNow,
            RiskScore = 0,
            RiskLevel = RiskLevel.Low,
            Verdict = ScanVerdict.Unknown,
            Confidence = 0m
        };
        _db.UrlScans.Add(scan);

        _db.UrlAnalyses.Add(ToEntity(scan.Id, analysis));
        _db.ScanEvents.Add(new ScanEvent
        {
            UrlScanId = scan.Id,
            Stage = ScanStatus.UrlAnalysis,
            Message = "URL-analysis stage completed."
        });
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId, Action = AuditAction.ScanCreated, EntityType = nameof(UrlScan), EntityId = scan.Id.ToString()
        });

        await _db.SaveChangesAsync(ct);
        _scanQueue.Enqueue(scan.Id);

        return await BuildDetailDtoAsync(scan.Id, ct) ?? throw new InvalidOperationException("Scan vanished immediately after creation.");
    }

    public Task<ScanDetailDto?> GetScanAsync(Guid scanId, CancellationToken ct = default) => BuildDetailDtoAsync(scanId, ct);

    public async Task<PagedResultDto<ScanSummaryDto>> GetScansAsync(int page, int pageSize, Guid? userId, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.UrlScans.AsQueryable();
        if (userId.HasValue) query = query.Where(s => s.UserId == userId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ScanSummaryDto(s.Id, s.OriginalUrl, s.Status, s.RiskScore, s.RiskLevel, s.Verdict, s.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResultDto<ScanSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<bool> DeleteScanAsync(Guid scanId, CancellationToken ct = default)
    {
        var scan = await _db.UrlScans.FirstOrDefaultAsync(s => s.Id == scanId, ct);
        if (scan is null) return false;

        scan.IsDeleted = true;
        scan.DeletedAtUtc = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = scan.UserId, Action = AuditAction.ScanDeleted, EntityType = nameof(UrlScan), EntityId = scan.Id.ToString()
        });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<ScanDetailDto?> BuildDetailDtoAsync(Guid scanId, CancellationToken ct)
    {
        var scan = await _db.UrlScans
            .Include(s => s.UrlAnalysis)
            .Include(s => s.DomainAnalysis)
            .Include(s => s.DnsAnalysis)
            .Include(s => s.SslAnalysis)
            .Include(s => s.RedirectAnalyses)
            .Include(s => s.ThreatIntelligenceResults).ThenInclude(r => r.Provider)
            .Include(s => s.BrandMatches).ThenInclude(m => m.BrandProfile)
            .Include(s => s.MlPrediction)
            .Include(s => s.RiskFactors)
            .FirstOrDefaultAsync(s => s.Id == scanId, ct);

        if (scan is null) return null;

        return new ScanDetailDto(
            scan.Id, scan.OriginalUrl, scan.NormalizedUrl, scan.Status, scan.FailureReason,
            scan.RiskScore, scan.RiskLevel, scan.Verdict, scan.Confidence,
            ToUrlAnalysisDto(scan.UrlAnalysis) is { } u ? u with { OriginalUrl = scan.OriginalUrl, NormalizedUrl = scan.NormalizedUrl } : null,
            ToDomainDto(scan.DomainAnalysis),
            ToDnsDto(scan.DnsAnalysis),
            ToSslDto(scan.SslAnalysis),
            ToRedirectDto(scan.RedirectAnalyses),
            scan.ThreatIntelligenceResults.Select(r => new ThreatIntelligenceResultDto(
                r.Provider.Name, r.Provider.Slug, r.Status, r.DetectionCategory, r.ErrorMessage, r.FromCache, r.CheckedAtUtc)).ToList(),
            scan.BrandMatches.Select(m => new BrandMatchResultDto(
                m.BrandProfileId, m.BrandProfile.BrandName, m.BrandProfile.OfficialDomain, m.MatchedDomain, m.MatchType,
                m.LevenshteinDistance, m.JaroWinklerSimilarity, m.ContainsAuthKeywords)).ToList(),
            scan.MlPrediction is { RequestSucceeded: true } ml ? new MlPredictionResultDto(ml.Prediction, ml.Probability, ml.ModelVersion) : null,
            scan.RiskFactors.Select(f => new RiskFactorDto(f.Category, f.Description, f.ScoreContribution)).ToList(),
            scan.CreatedAtUtc, scan.CompletedAtUtc);
    }

    private static UrlAnalysis ToEntity(Guid scanId, UrlAnalysisResultDto dto) => new()
    {
        UrlScanId = scanId,
        UrlLength = dto.UrlLength,
        DomainLength = dto.DomainLength,
        PathLength = dto.PathLength,
        QueryLength = dto.QueryLength,
        SubdomainCount = dto.SubdomainCount,
        DotCount = dto.DotCount,
        HyphenCount = dto.HyphenCount,
        DigitCount = dto.DigitCount,
        SpecialCharCount = dto.SpecialCharCount,
        IsIpAddressHost = dto.IsIpAddressHost,
        IsHttps = dto.IsHttps,
        HasUrlEncoding = dto.HasUrlEncoding,
        HasDoubleEncoding = dto.HasDoubleEncoding,
        HasPunycode = dto.HasPunycode,
        HasHomoglyphs = dto.HasHomoglyphs,
        IsShortenedUrl = dto.IsShortenedUrl,
        HasSuspiciousTld = dto.HasSuspiciousTld,
        HasRedirectParameter = dto.HasRedirectParameter,
        HasSuspiciousFileExtension = dto.HasSuspiciousFileExtension,
        SuspiciousKeywordsJson = JsonSerializer.Serialize(dto.SuspiciousKeywords),
        SuspiciousParametersJson = JsonSerializer.Serialize(dto.SuspiciousParameters),
        UrlAnalysisScore = dto.UrlAnalysisScore
    };

    private static UrlAnalysisResultDto? ToUrlAnalysisDto(UrlAnalysis? entity)
    {
        if (entity is null) return null;
        return new UrlAnalysisResultDto(
            OriginalUrl: string.Empty, NormalizedUrl: string.Empty,
            UrlLength: entity.UrlLength, DomainLength: entity.DomainLength, PathLength: entity.PathLength, QueryLength: entity.QueryLength,
            SubdomainCount: entity.SubdomainCount, DotCount: entity.DotCount, HyphenCount: entity.HyphenCount,
            DigitCount: entity.DigitCount, SpecialCharCount: entity.SpecialCharCount,
            IsIpAddressHost: entity.IsIpAddressHost, IsHttps: entity.IsHttps, HasUrlEncoding: entity.HasUrlEncoding,
            HasDoubleEncoding: entity.HasDoubleEncoding, HasPunycode: entity.HasPunycode, HasHomoglyphs: entity.HasHomoglyphs,
            IsShortenedUrl: entity.IsShortenedUrl, HasSuspiciousTld: entity.HasSuspiciousTld,
            HasRedirectParameter: entity.HasRedirectParameter, HasSuspiciousFileExtension: entity.HasSuspiciousFileExtension,
            SuspiciousKeywords: JsonSerializer.Deserialize<List<string>>(entity.SuspiciousKeywordsJson) ?? [],
            SuspiciousParameters: JsonSerializer.Deserialize<List<string>>(entity.SuspiciousParametersJson) ?? [],
            UrlAnalysisScore: entity.UrlAnalysisScore);
    }

    private static DomainAnalysisResultDto? ToDomainDto(DomainAnalysis? e) => e is null ? null : new DomainAnalysisResultDto(
        e.Domain, e.RegisteredDomain, e.Subdomain, e.Tld, e.Registrar, e.RegisteredAtUtc, e.ExpiresAtUtc, e.DomainAgeDays,
        e.Organization, e.Country, JsonSerializer.Deserialize<List<string>>(e.NameserversJson) ?? [],
        e.RdapLookupSucceeded, e.RdapError, e.DomainAnalysisScore);

    private static DnsAnalysisResultDto? ToDnsDto(DnsAnalysis? e) => e is null ? null : new DnsAnalysisResultDto(
        e.Resolves,
        JsonSerializer.Deserialize<List<string>>(e.ARecordsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(e.AaaaRecordsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(e.MxRecordsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(e.NsRecordsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(e.CnameRecordsJson) ?? [],
        JsonSerializer.Deserialize<List<string>>(e.TxtRecordsJson) ?? [],
        e.HasMailConfiguration, e.HasSuspiciousNameservers, e.LookupError, e.DnsAnalysisScore);

    private static SslAnalysisResultDto? ToSslDto(SslAnalysis? e) => e is null ? null : new SslAnalysisResultDto(
        e.HasCertificate, e.IsValid, e.Issuer, e.Subject,
        JsonSerializer.Deserialize<List<string>>(e.SubjectAlternativeNamesJson) ?? [],
        e.ValidFromUtc, e.ValidToUtc, e.IsExpired, e.IsSelfSigned, e.DomainMatchesCertificate,
        e.TlsVersion, e.ChainValidationError, e.SslAnalysisScore);

    private static RedirectAnalysisResultDto? ToRedirectDto(ICollection<RedirectAnalysis> hops)
    {
        if (hops.Count == 0) return null;
        var ordered = hops.OrderBy(h => h.HopIndex).ToList();
        var hopDtos = ordered.Select(h => new RedirectHopDto(
            h.HopIndex, h.FromUrl, h.ToUrl, h.StatusCode, h.DomainChanged, h.HttpsDowngrade, h.TargetIsSuspicious)).ToList();

        return new RedirectAnalysisResultDto(
            FinalUrl: ordered[^1].ToUrl,
            RedirectCount: hopDtos.Count,
            AnyDomainChange: hopDtos.Any(h => h.DomainChanged),
            AnyHttpsDowngrade: hopDtos.Any(h => h.HttpsDowngrade),
            Hops: hopDtos,
            Error: null,
            RedirectAnalysisScore: 0);
    }
}
