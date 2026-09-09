using FoodOrder.Application.DTOs.Delivery;

namespace FoodOrder.Application.Interfaces.Services;

public interface IDeliveryService
{
    // Dashboard
    Task<DeliveryDashboardDto> GetDashboardAsync(int? organizationId);

    // Drivers
    Task<IReadOnlyList<DriverDto>> GetDriversAsync(int? organizationId);
    Task<DriverDto> CreateDriverAsync(CreateDriverDto dto, int? organizationId);
    Task<DriverDto?> UpdateDriverAsync(int id, UpdateDriverDto dto);
    Task<DriverDto?> UpdateDriverLocationAsync(int id, UpdateDriverLocationDto dto);
    Task DeleteDriverAsync(int id);

    // Delivery Orders
    Task<IReadOnlyList<DeliveryOrderDto>> GetDeliveryOrdersAsync(int? organizationId, DateTime? date = null);
    Task<DeliveryOrderDto?> GetDeliveryOrderAsync(int id);
    Task<DeliveryOrderDto> CreateDeliveryOrderAsync(CreateDeliveryOrderDto dto);
    Task<DeliveryOrderDto?> AssignDriverAsync(int deliveryId, AssignDriverDto dto);
    Task<DeliveryOrderDto?> UpdateStatusAsync(int deliveryId, UpdateDeliveryStatusDto dto);

    // Charge Slabs
    Task<IReadOnlyList<DeliveryChargeSlabDto>> GetChargeSlabsAsync(int? organizationId);
    Task<DeliveryChargeSlabDto> UpsertChargeSlabAsync(UpsertDeliveryChargeSlabDto dto, int? organizationId);
    Task DeleteChargeSlabAsync(int id);

    // Third Party Configs
    Task<IReadOnlyList<ThirdPartyConfigDto>> GetThirdPartyConfigsAsync(int? organizationId);
    Task<ThirdPartyConfigDto> UpsertThirdPartyConfigAsync(UpsertThirdPartyConfigDto dto, int? organizationId);

    // Third Party Dispatch
    Task<DeliveryOrderDto?> DispatchToThirdPartyAsync(int deliveryId, string provider, int? organizationId);
}
