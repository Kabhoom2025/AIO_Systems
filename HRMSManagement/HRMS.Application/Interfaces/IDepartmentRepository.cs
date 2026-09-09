using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllByOrgAsync(int orgId);
    Task<Department?> GetByIdAsync(int orgId, int id);
    Task<Dictionary<int, int>> GetEmployeeCountsAsync(int orgId);
    Task<int> GetEmployeeCountAsync(int departmentId);
    void Add(Department department);
    void Update(Department department);
    void Remove(Department department);
    Task SaveChangesAsync();
}
