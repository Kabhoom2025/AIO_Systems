using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IStoreRepository
{
    Task<List<Store>> GetAllByOrgAsync(int orgId);
    Task<Store?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(Store store);
    void Update(Store store);
    void Remove(Store store);
    Task SaveChangesAsync();
}
