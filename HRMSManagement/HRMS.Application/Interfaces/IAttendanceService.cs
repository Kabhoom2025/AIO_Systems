using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceRecordDto> CheckInAsync(int orgId, int employeeId, CheckInDto dto);
    Task<AttendanceRecordDto> CheckOutAsync(int orgId, int employeeId);
    Task<AttendanceRecordDto?> GetTodayAsync(int orgId, int employeeId);
    Task<List<AttendanceDayDto>> GetDailyAsync(int orgId, DateOnly date);
    Task<List<AttendanceRecordDto>> GetByEmployeeMonthAsync(int orgId, int employeeId, int year, int month);
    Task<AttendanceSummaryDto> GetSummaryAsync(int orgId, DateOnly date);

    Task<RegularizationDto> CreateRegularizationAsync(int orgId, int employeeId, CreateRegularizationDto dto);
    Task<List<RegularizationDto>> GetRegularizationsAsync(int orgId);
    Task<List<RegularizationDto>> GetMyRegularizationsAsync(int orgId, int employeeId);
    Task<RegularizationDto> ApproveRegularizationAsync(int orgId, int id, int reviewerUserId, ReviewDto dto);
    Task<RegularizationDto> RejectRegularizationAsync(int orgId, int id, int reviewerUserId, ReviewDto dto);
}
