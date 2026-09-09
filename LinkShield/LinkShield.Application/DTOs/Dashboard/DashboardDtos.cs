namespace LinkShield.Application.DTOs.Dashboard;

public record DashboardSummaryDto(
    int TotalScans,
    int SafeCount,
    int SuspiciousCount,
    int MaliciousCount,
    int CriticalCount,
    int ThreatsDetected,
    double AverageRiskScore,
    int ThreatIntelligenceMatches);

public record ScansOverTimePointDto(DateOnly Date, int TotalScans, int SafeCount, int SuspiciousCount, int MaliciousCount);

public record RiskDistributionPointDto(string RiskLevel, int Count);

public record ThreatCategoryPointDto(string Category, int Count);

public record TopDomainPointDto(string Domain, int Count, int AverageRiskScore);

public record TopBrandPointDto(string BrandName, int Count);

public record DashboardTrendsDto(
    IReadOnlyList<ScansOverTimePointDto> ScansOverTime,
    IReadOnlyList<RiskDistributionPointDto> RiskDistribution,
    IReadOnlyList<ThreatCategoryPointDto> ThreatCategories,
    IReadOnlyList<TopDomainPointDto> TopSuspiciousDomains,
    IReadOnlyList<TopBrandPointDto> TopTargetedBrands);
