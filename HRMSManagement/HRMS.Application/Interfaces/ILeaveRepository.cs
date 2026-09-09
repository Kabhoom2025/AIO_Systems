using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface ILeaveRepository
{
    // Leave types
    Task<List<LeaveType>> GetAllTypesByOrgAsync(int orgId);
    Task<LeaveType?> GetTypeByIdAsync(int orgId, int id);
    Task<List<LeaveType>> GetActiveTypesAsync(int orgId);
    Task<bool> TypeHasRequestsOrBalancesAsync(int typeId);
    void AddType(LeaveType type);
    void UpdateType(LeaveType type);
    void RemoveType(LeaveType type);

    // Balances
    Task<List<LeaveBalance>> GetBalancesByEmployeeYearAsync(int orgId, int employeeId, int year);
    Task<LeaveBalance?> GetBalanceAsync(int orgId, int employeeId, int leaveTypeId, int year);
    Task<List<LeaveBalance>> GetBalancesByYearAsync(int orgId, int year);
    Task<List<Employee>> GetActiveEmployeesAsync(int orgId);
    void AddBalance(LeaveBalance balance);
    void UpdateBalance(LeaveBalance balance);

    // Requests
    Task<LeaveRequest?> GetRequestByIdAsync(int orgId, int id);
    Task<int?> GetUserIdByEmployeeIdAsync(int? employeeId);
    Task<List<LeaveRequest>> GetRequestsByEmployeeAsync(int orgId, int employeeId);
    Task<List<LeaveRequest>> GetRequestsForOrgAsync(int orgId, string? status);
    Task<List<LeaveRequest>> GetPendingRequestsAsync(int orgId);
    Task<List<LeaveRequest>> GetApprovedRequestsInRangeAsync(int orgId, DateOnly from, DateOnly to);
    void AddRequest(LeaveRequest request);
    void UpdateRequest(LeaveRequest request);

    Task SaveChangesAsync();
}
