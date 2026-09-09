using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllByOrgAsync(int orgId);
    Task<Department?> GetByIdAsync(int orgId, int id);
    void Add(Department department);
    void Update(Department department);
    void Remove(Department department);
    Task SaveChangesAsync();
}
