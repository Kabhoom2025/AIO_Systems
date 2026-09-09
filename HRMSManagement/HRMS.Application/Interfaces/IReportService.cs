using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IReportService
{
    Task<List<HeadcountReportRow>> GetHeadcountReportAsync(int orgId);
    Task<List<AttendanceMonthlyReportRow>> GetAttendanceMonthlyReportAsync(int orgId, int year, int month);
    Task<List<LeaveReportRow>> GetLeaveReportAsync(int orgId, int year);
    Task<List<PayrollSummaryReportRow>> GetPayrollSummaryReportAsync(int orgId, int year);
    Task<List<RecruitmentPipelineReportRow>> GetRecruitmentPipelineReportAsync(int orgId);
    Task<List<AssetReportRow>> GetAssetsReportAsync(int orgId);
}
