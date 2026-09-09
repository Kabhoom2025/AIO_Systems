using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IDeliveryLoadService
{
    Task<List<DeliveryLoadDto>> GetAllAsync(int orgId);
    Task<DeliveryLoadDto> GetByIdAsync(int orgId, int id);
    /// <summary>Get-or-create: returns the vehicle's already-in-progress (undispatched) load if
    /// one exists, otherwise creates a new one — so re-selecting a vehicle in the UI always
    /// resumes the same load instead of creating duplicates.</summary>
    Task<DeliveryLoadDto> CreateAsync(int orgId, CreateDeliveryLoadDto dto);
    Task<List<DeliveryLoadShipmentDto>> GetAvailableShipmentsAsync(int orgId, int warehouseId);
    Task<DeliveryLoadDto> AssignShipmentAsync(int orgId, int loadId, AssignShipmentDto dto);
    Task<DeliveryLoadDto> UnassignShipmentAsync(int orgId, int loadId, int shipmentId);
    Task<DeliveryLoadDto> DispatchAsync(int orgId, int id);
}
