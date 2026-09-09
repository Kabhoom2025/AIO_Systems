using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
    void Add(Permission permission);
    void Update(Permission permission);
    void Remove(Permission permission);
    Task SaveChangesAsync();
}
