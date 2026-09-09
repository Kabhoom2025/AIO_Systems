using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IDeliveryRepository
{
    // Drivers
    Task<IReadOnlyList<Driver>> GetAllDriversAsync(int? organizationId);
    Task<Driver?> GetDriverByIdAsync(int id);
    Task AddDriverAsync(Driver driver);
    void UpdateDriver(Driver driver);
    Task SaveChangesAsync();

    // Delivery Orders
    Task<IReadOnlyList<DeliveryOrder>> GetAllDeliveryOrdersAsync(int? organizationId, DateTime? date = null);
    Task<DeliveryOrder?> GetDeliveryOrderByIdAsync(int id);
    Task<DeliveryOrder?> GetDeliveryOrderByOrderIdAsync(int orderId);
    Task AddDeliveryOrderAsync(DeliveryOrder delivery);

    // Charge Slabs
    Task<IReadOnlyList<DeliveryChargeSlab>> GetChargeSlabsAsync(int? organizationId);
    Task<DeliveryChargeSlab?> GetChargeSlabByIdAsync(int id);
    Task AddChargeSlabAsync(DeliveryChargeSlab slab);
    void DeleteChargeSlab(DeliveryChargeSlab slab);

    // Third Party Configs
    Task<IReadOnlyList<ThirdPartyDeliveryConfig>> GetThirdPartyConfigsAsync(int? organizationId);
    Task<ThirdPartyDeliveryConfig?> GetThirdPartyConfigByProviderAsync(string provider, int? organizationId);
    Task AddThirdPartyConfigAsync(ThirdPartyDeliveryConfig config);
}
