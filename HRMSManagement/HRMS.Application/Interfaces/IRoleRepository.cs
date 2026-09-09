using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllForOrgAsync(int orgId);
    Task<Role?> GetByIdAsync(int id);
    Task<List<Permission>> GetPermissionsByKeysAsync(List<string> keys);
    void Add(Role role);
    void Update(Role role);
    void Remove(Role role);
    Task SaveChangesAsync();
}
