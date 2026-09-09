using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IAssetRepository
{
    Task<List<Asset>> GetAllByOrgAsync(int orgId);
    Task<Asset?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(Asset asset);
    void Update(Asset asset);
    void Remove(Asset asset);
    Task SaveChangesAsync();
}
