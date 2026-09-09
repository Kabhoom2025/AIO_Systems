using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IStoreService
{
    Task<List<StoreDto>> GetAllAsync(int orgId);
    Task<StoreDto> GetByIdAsync(int orgId, int id);
    Task<StoreDto> CreateAsync(int orgId, CreateStoreDto dto);
    Task<StoreDto> UpdateAsync(int orgId, int id, UpdateStoreDto dto);
    Task DeleteAsync(int orgId, int id);
}
