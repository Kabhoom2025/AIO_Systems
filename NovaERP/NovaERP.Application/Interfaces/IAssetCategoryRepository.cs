using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IAssetCategoryRepository
{
    Task<List<AssetCategory>> GetAllByOrgAsync(int orgId);
    Task<AssetCategory?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(AssetCategory category);
    void Update(AssetCategory category);
    void Remove(AssetCategory category);
    Task SaveChangesAsync();
}
