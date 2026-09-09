using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IShipmentService
{
    Task<List<ShipmentDto>> GetAllAsync(int orgId);
    Task<ShipmentDto> GetByIdAsync(int orgId, int id);
    Task<ShipmentDto> CreateAsync(int orgId, CreateShipmentDto dto);
    Task<ShipmentDto> UpdateAsync(int orgId, int id, UpdateShipmentDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<ShipmentDto> PickAsync(int orgId, int id);
    Task<ShipmentDto> ShipAsync(int orgId, int id);
    Task<ShipmentDto> DeliverAsync(int orgId, int id);
    Task<ShipmentDto> CancelAsync(int orgId, int id);
}
