using LinkShield.Application.DTOs.Dashboard;
using LinkShield.Application.Interfaces;
using LinkShield.Domain.Enums;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly LinkShieldDbContext _db;

    public DashboardService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var totalScans = await _db.UrlScans.CountAsync(ct);
        var safeCount = await _db.UrlScans.CountAsync(s => s.Verdict == ScanVerdict.Safe, ct);
        var suspiciousCount = await _db.UrlScans.CountAsync(s => s.Verdict == ScanVerdict.Suspicious, ct);
        var maliciousCount = await _db.UrlScans.CountAsync(s => s.Verdict == ScanVerdict.Malicious, ct);
        var criticalCount = await _db.UrlScans.CountAsync(s => s.RiskLevel == RiskLevel.Critical, ct);
        var threatIntelligenceMatches = await _db.ThreatIntelligenceResults
            .CountAsync(r => r.Status == ThreatIntelligenceStatus.ConfirmedMalicious, ct);

        var completedScores = await _db.UrlScans
            .Where(s => s.Status == ScanStatus.Completed)
            .Select(s => s.RiskScore)
            .ToListAsync(ct);
        var averageRiskScore = completedScores.Count == 0 ? 0 : completedScores.Average();

        return new DashboardSummaryDto(
            totalScans, safeCount, suspiciousCount, maliciousCount, criticalCount,
            ThreatsDetected: maliciousCount, averageRiskScore, threatIntelligenceMatches);
    }

    public async Task<DashboardTrendsDto> GetTrendsAsync(int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 90);
        var since = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var recentScans = await _db.UrlScans
            .Where(s => s.CreatedAtUtc >= since)
            .Select(s => new { s.CreatedAtUtc, s.Verdict, s.RiskLevel, s.NormalizedUrl, s.RiskScore, s.Status })
            .ToListAsync(ct);

        var scansOverTime = Enumerable.Range(0, days)
            .Select(offset => DateOnly.FromDateTime(since.AddDays(offset)))
            .Select(date =>
            {
                var dayScans = recentScans.Where(s => DateOnly.FromDateTime(s.CreatedAtUtc) == date).ToList();
                return new ScansOverTimePointDto(
                    date, dayScans.Count,
                    dayScans.Count(s => s.Verdict == ScanVerdict.Safe),
                    dayScans.Count(s => s.Verdict == ScanVerdict.Suspicious),
                    dayScans.Count(s => s.Verdict == ScanVerdict.Malicious));
            })
            .ToList();

        var riskDistribution = recentScans
            .Where(s => s.Status == ScanStatus.Completed)
            .GroupBy(s => s.RiskLevel)
            .Select(g => new RiskDistributionPointDto(g.Key.ToString(), g.Count()))
            .ToList();

        var threatCategories = await _db.ThreatIntelligenceResults
            .Where(r => r.Status == ThreatIntelligenceStatus.ConfirmedMalicious && r.CheckedAtUtc >= since && r.DetectionCategory != null)
            .GroupBy(r => r.DetectionCategory)
            .Select(g => new { Category = g.Key!, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToListAsync(ct);

        // Keyed off RiskScore rather than Verdict: a low-confidence scan (e.g. ml-service
        // unreachable) can carry a meaningful score while still reporting Verdict=Unknown —
        // that scan's evidence is still worth surfacing here.
        var topSuspiciousDomains = recentScans
            .Where(s => s.RiskScore > 0)
            .Select(s => (Host: TryGetHost(s.NormalizedUrl), s.RiskScore))
            .Where(s => s.Host is not null)
            .GroupBy(s => s.Host!)
            .Select(g => new TopDomainPointDto(g.Key, g.Count(), (int)Math.Round(g.Average(x => x.RiskScore))))
            .OrderByDescending(d => d.Count)
            .Take(10)
            .ToList();

        var topTargetedBrands = await _db.BrandMatches
            .Where(m => m.CreatedAtUtc >= since)
            .Include(m => m.BrandProfile)
            .GroupBy(m => m.BrandProfile.BrandName)
            .Select(g => new { BrandName = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToListAsync(ct);

        return new DashboardTrendsDto(
            scansOverTime,
            riskDistribution,
            threatCategories.Select(t => new ThreatCategoryPointDto(t.Category, t.Count)).ToList(),
            topSuspiciousDomains,
            topTargetedBrands.Select(b => new TopBrandPointDto(b.BrandName, b.Count)).ToList());
    }

    private static string? TryGetHost(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : null;
}
