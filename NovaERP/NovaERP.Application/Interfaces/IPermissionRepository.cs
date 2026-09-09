using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
    void Add(Permission permission);
    void Update(Permission permission);
    void Remove(Permission permission);
    Task SaveChangesAsync();
}
