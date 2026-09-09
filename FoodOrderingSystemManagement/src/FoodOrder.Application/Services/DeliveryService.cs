using FoodOrder.Application.DTOs.Delivery;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _repo;

    public DeliveryService(IDeliveryRepository repo) => _repo = repo;

    // ── Dashboard ────────────────────────────────────────────────────────────

    public async Task<DeliveryDashboardDto> GetDashboardAsync(int? organizationId)
    {
        var today     = DateTime.UtcNow.Date;
        var all       = await _repo.GetAllDeliveryOrdersAsync(organizationId, today);
        var drivers   = await _repo.GetAllDriversAsync(organizationId);

        return new DeliveryDashboardDto
        {
            TotalToday       = all.Count,
            PendingCount     = all.Count(d => d.Status == DeliveryStatus.Pending),
            ActiveCount      = all.Count(d => d.Status is DeliveryStatus.Assigned
                                           or DeliveryStatus.PickedUp
                                           or DeliveryStatus.OutForDelivery),
            DeliveredToday   = all.Count(d => d.Status == DeliveryStatus.Delivered),
            FailedToday      = all.Count(d => d.Status is DeliveryStatus.Failed or DeliveryStatus.Cancelled),
            AvailableDrivers = drivers.Count(d => d.IsAvailable && d.IsActive),
            TotalChargeToday = all.Sum(d => d.DeliveryCharge),
        };
    }

    // ── Drivers ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(int? organizationId)
    {
        var drivers = await _repo.GetAllDriversAsync(organizationId);
        var all     = await _repo.GetAllDeliveryOrdersAsync(organizationId);
        var activeCounts = all
            .Where(d => d.DriverId.HasValue && d.Status is DeliveryStatus.Assigned
                                            or DeliveryStatus.PickedUp
                                            or DeliveryStatus.OutForDelivery)
            .GroupBy(d => d.DriverId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return drivers.Select(d => MapDriver(d, activeCounts.GetValueOrDefault(d.Id, 0))).ToList();
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverDto dto, int? organizationId)
    {
        var driver = new Driver
        {
            Name           = dto.Name.Trim(),
            Phone          = dto.Phone.Trim(),
            Email          = dto.Email?.Trim(),
            VehicleNo      = dto.VehicleNo?.Trim(),
            VehicleType    = dto.VehicleType?.Trim(),
            OrganizationId = organizationId,
            CreatedDate    = DateTime.UtcNow,
        };
        await _repo.AddDriverAsync(driver);
        await _repo.SaveChangesAsync();
        return MapDriver(driver, 0);
    }

    public async Task<DriverDto?> UpdateDriverAsync(int id, UpdateDriverDto dto)
    {
        var driver = await _repo.GetDriverByIdAsync(id);
        if (driver is null) return null;

        driver.Name        = dto.Name.Trim();
        driver.Phone       = dto.Phone.Trim();
        driver.Email       = dto.Email?.Trim();
        driver.VehicleNo   = dto.VehicleNo?.Trim();
        driver.VehicleType = dto.VehicleType?.Trim();
        driver.IsAvailable = dto.IsAvailable;
        driver.IsActive    = dto.IsActive;
        _repo.UpdateDriver(driver);
        await _repo.SaveChangesAsync();
        return MapDriver(driver, 0);
    }

    public async Task DeleteDriverAsync(int id)
    {
        var driver = await _repo.GetDriverByIdAsync(id)
            ?? throw new NotFoundException("Driver", id);
        driver.IsActive = false;
        _repo.UpdateDriver(driver);
        await _repo.SaveChangesAsync();
    }

    // ── Delivery Orders ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DeliveryOrderDto>> GetDeliveryOrdersAsync(int? organizationId, DateTime? date = null)
    {
        var list = await _repo.GetAllDeliveryOrdersAsync(organizationId, date);
        return list.Select(MapDelivery).ToList();
    }

    public async Task<DeliveryOrderDto?> GetDeliveryOrderAsync(int id)
    {
        var d = await _repo.GetDeliveryOrderByIdAsync(id);
        return d is null ? null : MapDelivery(d);
    }

    public async Task<DeliveryOrderDto> CreateDeliveryOrderAsync(CreateDeliveryOrderDto dto)
    {
        var delivery = new DeliveryOrder
        {
            OrderId           = dto.OrderId,
            DeliveryAddress   = dto.DeliveryAddress.Trim(),
            DeliveryLatitude  = dto.DeliveryLatitude,
            DeliveryLongitude = dto.DeliveryLongitude,
            DeliveryCharge    = dto.DeliveryCharge,
            CustomerPhone   = dto.CustomerPhone?.Trim(),
            Notes           = dto.Notes?.Trim(),
            Status          = DeliveryStatus.Pending,
            CreatedDate     = DateTime.UtcNow,
        };
        await _repo.AddDeliveryOrderAsync(delivery);
        await _repo.SaveChangesAsync();

        var saved = await _repo.GetDeliveryOrderByIdAsync(delivery.Id);
        return MapDelivery(saved!);
    }

    public async Task<DeliveryOrderDto?> AssignDriverAsync(int deliveryId, AssignDriverDto dto)
    {
        var delivery = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        if (delivery is null) return null;

        var driver = await _repo.GetDriverByIdAsync(dto.DriverId)
            ?? throw new AppException("Driver not found.");

        delivery.DriverId   = dto.DriverId;
        delivery.Status     = DeliveryStatus.Assigned;
        delivery.AssignedAt = DateTime.UtcNow;
        driver.IsAvailable  = false;
        _repo.UpdateDriver(driver);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        return MapDelivery(updated!);
    }

    public async Task<DeliveryOrderDto?> UpdateStatusAsync(int deliveryId, UpdateDeliveryStatusDto dto)
    {
        var delivery = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        if (delivery is null) return null;

        if (!Enum.TryParse<DeliveryStatus>(dto.Status, out var newStatus))
            throw new AppException($"Invalid delivery status: {dto.Status}");

        delivery.Status = newStatus;
        if (newStatus == DeliveryStatus.PickedUp)     delivery.PickedUpAt  = DateTime.UtcNow;
        if (newStatus == DeliveryStatus.Delivered)    delivery.DeliveredAt = DateTime.UtcNow;
        if (dto.Notes is not null) delivery.Notes = dto.Notes;

        // Free driver when terminal state reached
        if (newStatus is DeliveryStatus.Delivered or DeliveryStatus.Failed or DeliveryStatus.Cancelled)
        {
            if (delivery.DriverId.HasValue)
            {
                var driver = await _repo.GetDriverByIdAsync(delivery.DriverId.Value);
                if (driver is not null) { driver.IsAvailable = true; _repo.UpdateDriver(driver); }
            }
        }

        await _repo.SaveChangesAsync();
        var updated = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        return MapDelivery(updated!);
    }

    // ── Charge Slabs ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DeliveryChargeSlabDto>> GetChargeSlabsAsync(int? organizationId)
    {
        var list = await _repo.GetChargeSlabsAsync(organizationId);
        return list.Select(s => new DeliveryChargeSlabDto
            { Id = s.Id, FromKm = s.FromKm, ToKm = s.ToKm, Charge = s.Charge }).ToList();
    }

    public async Task<DeliveryChargeSlabDto> UpsertChargeSlabAsync(UpsertDeliveryChargeSlabDto dto, int? organizationId)
    {
        var slab = new DeliveryChargeSlab
        {
            FromKm         = dto.FromKm,
            ToKm           = dto.ToKm,
            Charge         = dto.Charge,
            OrganizationId = organizationId,
            CreatedDate    = DateTime.UtcNow,
        };
        await _repo.AddChargeSlabAsync(slab);
        await _repo.SaveChangesAsync();
        return new DeliveryChargeSlabDto { Id = slab.Id, FromKm = slab.FromKm, ToKm = slab.ToKm, Charge = slab.Charge };
    }

    public async Task DeleteChargeSlabAsync(int id)
    {
        var slab = await _repo.GetChargeSlabByIdAsync(id)
            ?? throw new NotFoundException("DeliveryChargeSlab", id);
        _repo.DeleteChargeSlab(slab);
        await _repo.SaveChangesAsync();
    }

    // ── Third Party Configs ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<ThirdPartyConfigDto>> GetThirdPartyConfigsAsync(int? organizationId)
    {
        var list = await _repo.GetThirdPartyConfigsAsync(organizationId);
        return list.Select(MapThirdParty).ToList();
    }

    public async Task<ThirdPartyConfigDto> UpsertThirdPartyConfigAsync(UpsertThirdPartyConfigDto dto, int? organizationId)
    {
        var existing = await _repo.GetThirdPartyConfigByProviderAsync(dto.Provider, organizationId);
        if (existing is not null)
        {
            existing.ApiKey     = dto.ApiKey.Trim();
            existing.ApiSecret  = dto.ApiSecret?.Trim();
            existing.WebhookUrl = dto.WebhookUrl?.Trim();
            existing.IsEnabled  = dto.IsEnabled;
            await _repo.SaveChangesAsync();
            return MapThirdParty(existing);
        }

        var config = new ThirdPartyDeliveryConfig
        {
            Provider       = dto.Provider.Trim(),
            ApiKey         = dto.ApiKey.Trim(),
            ApiSecret      = dto.ApiSecret?.Trim(),
            WebhookUrl     = dto.WebhookUrl?.Trim(),
            IsEnabled      = dto.IsEnabled,
            OrganizationId = organizationId,
            CreatedDate    = DateTime.UtcNow,
        };
        await _repo.AddThirdPartyConfigAsync(config);
        await _repo.SaveChangesAsync();
        return MapThirdParty(config);
    }

    // ── Third Party Dispatch ─────────────────────────────────────────────────

    public async Task<DeliveryOrderDto?> DispatchToThirdPartyAsync(int deliveryId, string provider, int? organizationId)
    {
        var delivery = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        if (delivery is null) return null;

        var config = await _repo.GetThirdPartyConfigByProviderAsync(provider, organizationId)
            ?? throw new AppException($"No active configuration found for provider '{provider}'. Please set up in Integrations.");

        if (!config.IsEnabled)
            throw new AppException($"'{provider}' integration is disabled. Enable it in Integrations settings first.");

        // In a real system, you would call config.ApiKey + provider's REST API here.
        // For now, we record the dispatch and mark the delivery order accordingly.
        var trackId = $"{provider.ToUpper()[0..3]}-{DateTime.UtcNow:yyyyMMddHHmmss}-{delivery.OrderId}";

        delivery.ThirdPartyProvider = provider;
        delivery.ThirdPartyTrackId  = trackId;
        delivery.Status             = DeliveryStatus.Assigned;
        delivery.AssignedAt         = DateTime.UtcNow;
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetDeliveryOrderByIdAsync(deliveryId);
        return MapDelivery(updated!);
    }

    // ── Mappers ──────────────────────────────────────────────────────────────

    public async Task<DriverDto?> UpdateDriverLocationAsync(int id, UpdateDriverLocationDto dto)
    {
        var driver = await _repo.GetDriverByIdAsync(id);
        if (driver is null) return null;
        driver.CurrentLatitude    = dto.Latitude;
        driver.CurrentLongitude   = dto.Longitude;
        driver.LastLocationUpdate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapDriver(driver, 0);
    }

    private static DriverDto MapDriver(Driver d, int active) => new()
    {
        Id = d.Id, Name = d.Name, Phone = d.Phone, Email = d.Email,
        VehicleNo = d.VehicleNo, VehicleType = d.VehicleType,
        IsAvailable = d.IsAvailable, IsActive = d.IsActive,
        ActiveDeliveries   = active,
        CurrentLatitude    = d.CurrentLatitude,
        CurrentLongitude   = d.CurrentLongitude,
        LastLocationUpdate = d.LastLocationUpdate,
    };

    private static DeliveryOrderDto MapDelivery(DeliveryOrder d) => new()
    {
        Id = d.Id, OrderId = d.OrderId,
        OrderNumber       = d.Order?.OrderNumber ?? string.Empty,
        OrderTotal        = d.Order?.GrandTotal ?? 0,
        DriverId          = d.DriverId,
        DriverName        = d.Driver?.Name,
        DriverPhone       = d.Driver?.Phone,
        Status            = d.Status.ToString(),
        DeliveryAddress   = d.DeliveryAddress,
        DeliveryLatitude  = d.DeliveryLatitude,
        DeliveryLongitude = d.DeliveryLongitude,
        DeliveryCharge    = d.DeliveryCharge,
        CustomerPhone     = d.CustomerPhone,
        Notes             = d.Notes,
        AssignedAt        = d.AssignedAt,
        PickedUpAt        = d.PickedUpAt,
        DeliveredAt       = d.DeliveredAt,
        ThirdPartyProvider = d.ThirdPartyProvider,
        ThirdPartyTrackId  = d.ThirdPartyTrackId,
        CreatedDate       = d.CreatedDate,
    };

    private static ThirdPartyConfigDto MapThirdParty(ThirdPartyDeliveryConfig c) => new()
    {
        Id = c.Id, Provider = c.Provider, ApiKey = c.ApiKey,
        ApiSecret = c.ApiSecret, WebhookUrl = c.WebhookUrl, IsEnabled = c.IsEnabled,
    };
}
