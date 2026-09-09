using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetAllAsync(int orgId);
    Task<VehicleDto> GetByIdAsync(int orgId, int id);
    Task<VehicleDto> CreateAsync(int orgId, CreateVehicleDto dto);
    Task<VehicleDto> UpdateAsync(int orgId, int id, UpdateVehicleDto dto);
    Task DeleteAsync(int orgId, int id);
}
