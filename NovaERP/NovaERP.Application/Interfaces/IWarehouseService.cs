using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAllAsync(int orgId);
    Task<WarehouseDto> GetByIdAsync(int orgId, int id);
    Task<WarehouseDto> CreateAsync(int orgId, CreateWarehouseDto dto);
    Task<WarehouseDto> UpdateAsync(int orgId, int id, UpdateWarehouseDto dto);
    Task DeleteAsync(int orgId, int id);
}
