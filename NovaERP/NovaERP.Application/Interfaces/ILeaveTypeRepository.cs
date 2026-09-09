using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ILeaveTypeRepository
{
    Task<List<LeaveType>> GetAllByOrgAsync(int orgId);
    Task<LeaveType?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(LeaveType leaveType);
    void Update(LeaveType leaveType);
    void Remove(LeaveType leaveType);
    Task SaveChangesAsync();
}
