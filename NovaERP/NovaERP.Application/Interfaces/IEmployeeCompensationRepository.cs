using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IEmployeeCompensationRepository
{
    Task<List<EmployeeCompensation>> GetAllByOrgAsync(int orgId);
    Task<EmployeeCompensation?> GetByIdAsync(int orgId, int id);
    Task<EmployeeCompensation?> GetByEmployeeIdAsync(int orgId, int employeeId);
    Task<bool> EmployeeIdExistsAsync(int orgId, int employeeId);

    void Add(EmployeeCompensation compensation);
    void Update(EmployeeCompensation compensation);
    void Remove(EmployeeCompensation compensation);
    Task SaveChangesAsync();
}
