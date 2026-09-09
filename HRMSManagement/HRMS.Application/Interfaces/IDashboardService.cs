using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int orgId);
    Task<MyDashboardDto> GetMyDashboardAsync(int orgId, int? employeeId);
}
