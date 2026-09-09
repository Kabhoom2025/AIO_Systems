using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAssetCategoryService
{
    Task<List<AssetCategoryDto>> GetAllAsync(int orgId);
    Task<AssetCategoryDto> GetByIdAsync(int orgId, int id);
    Task<AssetCategoryDto> CreateAsync(int orgId, CreateAssetCategoryDto dto);
    Task<AssetCategoryDto> UpdateAsync(int orgId, int id, UpdateAssetCategoryDto dto);
    Task DeleteAsync(int orgId, int id);
}
