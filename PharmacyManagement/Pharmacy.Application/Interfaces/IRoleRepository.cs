using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(int orgId);
    Task<Role?> GetByIdAsync(int id);
    Task<List<Permission>> GetPermissionsByKeysAsync(List<string> keys);
    void Add(Role role);
    void Remove(Role role);
    Task SaveChangesAsync();
}
