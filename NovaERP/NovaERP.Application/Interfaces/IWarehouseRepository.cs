using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IWarehouseRepository
{
    Task<List<Warehouse>> GetAllByOrgAsync(int orgId);
    Task<Warehouse?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(Warehouse warehouse);
    void Update(Warehouse warehouse);
    void Remove(Warehouse warehouse);
    Task SaveChangesAsync();
}
