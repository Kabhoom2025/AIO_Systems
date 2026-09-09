using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAssetService
{
    Task<List<AssetDto>> GetAllAsync(int orgId);
    Task<AssetDto> GetByIdAsync(int orgId, int id);
    Task<AssetDto> CreateAsync(int orgId, CreateAssetDto dto);
    Task<AssetDto> UpdateAsync(int orgId, int id, UpdateAssetDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<AssetDto> AssignAsync(int orgId, int id, AssignAssetDto dto);
    Task<AssetDto> UnassignAsync(int orgId, int id);
    Task<AssetDto> RetireAsync(int orgId, int id);
}
