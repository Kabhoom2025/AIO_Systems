using LinkShield.Application.DTOs.Dashboard;

namespace LinkShield.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<DashboardTrendsDto> GetTrendsAsync(int days, CancellationToken ct = default);
}
