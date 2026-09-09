using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IAssetService
{
    Task<List<AssetDto>> GetAllAsync(int orgId, string? category, string? status);
    Task<AssetDetailDto> GetByIdAsync(int orgId, int id);
    Task<AssetDto> CreateAsync(int orgId, CreateAssetDto dto);
    Task<AssetDto> UpdateAsync(int orgId, int id, UpdateAssetDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<AssetDto> AllocateAsync(int orgId, int id, int allocatedByUserId, AllocateAssetDto dto);
    Task<AssetDto> ReturnAsync(int orgId, int id, ReturnAssetDto dto);
    Task<List<AssetDto>> GetMyAssetsAsync(int orgId, int employeeId);
}
