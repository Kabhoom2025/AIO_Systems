using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ILeaveRequestRepository
{
    Task<List<LeaveRequest>> GetAllByOrgAsync(int orgId);
    Task<List<LeaveRequest>> GetAllByEmployeeIdAsync(int orgId, int employeeId);
    Task<LeaveRequest?> GetByIdAsync(int orgId, int id);

    void Add(LeaveRequest leaveRequest);
    void Update(LeaveRequest leaveRequest);
    void Remove(LeaveRequest leaveRequest);
    Task SaveChangesAsync();
}
