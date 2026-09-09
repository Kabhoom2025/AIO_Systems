using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllByOrgAsync(int orgId);
    Task<Employee?> GetByIdAsync(int orgId, int id);
    Task<Employee?> GetByUserIdAsync(int orgId, int userId);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(Employee employee);
    void Update(Employee employee);
    void Remove(Employee employee);
    Task SaveChangesAsync();
}
