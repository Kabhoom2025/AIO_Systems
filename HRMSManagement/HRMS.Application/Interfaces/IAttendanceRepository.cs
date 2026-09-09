using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<Employee?> GetEmployeeWithShiftAsync(int orgId, int employeeId);
    Task<int> GetActiveEmployeeCountAsync(int orgId);

    Task<AttendanceRecord?> GetByEmployeeAndDateAsync(int orgId, int employeeId, DateOnly date);
    Task<List<AttendanceRecord>> GetByDateAsync(int orgId, DateOnly date);
    Task<List<AttendanceRecord>> GetByEmployeeMonthAsync(int orgId, int employeeId, int year, int month);
    void AddRecord(AttendanceRecord record);
    void UpdateRecord(AttendanceRecord record);

    Task<AttendanceRegularization?> GetRegularizationByIdAsync(int orgId, int id);
    Task<List<AttendanceRegularization>> GetRegularizationsForOrgAsync(int orgId);
    Task<List<AttendanceRegularization>> GetRegularizationsByEmployeeAsync(int orgId, int employeeId);
    void AddRegularization(AttendanceRegularization regularization);
    void UpdateRegularization(AttendanceRegularization regularization);

    Task SaveChangesAsync();
}
