using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class DeliveryLoadService : IDeliveryLoadService
{
    private readonly IDeliveryLoadRepository _repo;
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IShipmentRepository _shipmentRepo;
    private readonly IShipmentService _shipmentService;
    private readonly IValidator<CreateDeliveryLoadDto> _createValidator;
    private readonly IValidator<AssignShipmentDto> _assignValidator;

    public DeliveryLoadService(IDeliveryLoadRepository repo, IVehicleRepository vehicleRepo,
        IShipmentRepository shipmentRepo, IShipmentService shipmentService,
        IValidator<CreateDeliveryLoadDto> createValidator, IValidator<AssignShipmentDto> assignValidator)
    {
        _repo = repo;
        _vehicleRepo = vehicleRepo;
        _shipmentRepo = shipmentRepo;
        _shipmentService = shipmentService;
        _createValidator = createValidator;
        _assignValidator = assignValidator;
    }

    public async Task<List<DeliveryLoadDto>> GetAllAsync(int orgId)
    {
        var loads = await _repo.GetAllByOrgAsync(orgId);
        return loads.Select(ToDto).ToList();
    }

    public async Task<DeliveryLoadDto> GetByIdAsync(int orgId, int id)
    {
        var load = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"DeliveryLoad {id} not found");
        return ToDto(load);
    }

    public async Task<DeliveryLoadDto> CreateAsync(int orgId, CreateDeliveryLoadDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var vehicle = await _vehicleRepo.GetByIdAsync(orgId, dto.VehicleId)
            ?? throw new KeyNotFoundException($"Vehicle {dto.VehicleId} not found");

        var existing = vehicle.DeliveryLoads.FirstOrDefault(l => l.DispatchedDate == null);
        if (existing != null)
        {
            var reloadedExisting = await _repo.GetByIdAsync(orgId, existing.Id) ?? existing;
            return ToDto(reloadedExisting);
        }

        var load = new DeliveryLoad
        {
            OrganizationId = orgId,
            VehicleId = dto.VehicleId,
            WarehouseId = dto.WarehouseId,
            LoadDate = DateTime.UtcNow.Date
        };

        _repo.Add(load);
        await _repo.SaveChangesAsync();

        // LoadNumber depends on the generated Id, so it's set in a second save — same scheme
        // as SalesOrder.OrderNumber/Shipment.ShipmentNumber.
        load.LoadNumber = $"DL-{load.Id:D5}";
        _repo.Update(load);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, load.Id) ?? load;
        return ToDto(reloaded);
    }

    public async Task<List<DeliveryLoadShipmentDto>> GetAvailableShipmentsAsync(int orgId, int warehouseId)
    {
        var shipments = await _shipmentRepo.GetPickedUnassignedByWarehouseAsync(orgId, warehouseId);
        return shipments.Select(ToShipmentDto).ToList();
    }

    public async Task<DeliveryLoadDto> AssignShipmentAsync(int orgId, int loadId, AssignShipmentDto dto)
    {
        await _assignValidator.ValidateAndThrowAsync(dto);

        var load = await _repo.GetByIdAsync(orgId, loadId)
            ?? throw new KeyNotFoundException($"DeliveryLoad {loadId} not found");
        if (load.DispatchedDate != null)
            throw new InvalidOperationException("This delivery load has already been dispatched.");

        var shipment = await _shipmentRepo.GetByIdAsync(orgId, dto.ShipmentId)
            ?? throw new KeyNotFoundException($"Shipment {dto.ShipmentId} not found");

        if (shipment.WarehouseId != load.WarehouseId)
            throw new InvalidOperationException("This shipment isn't from the same warehouse as the delivery load.");
        if (shipment.Status != "Picked")
            throw new InvalidOperationException("Only Picked shipments can be loaded onto a vehicle.");
        if (shipment.DeliveryLoadId != null && shipment.DeliveryLoadId != loadId)
            throw new InvalidOperationException("This shipment is already assigned to a different delivery load.");

        var currentWeight = load.Shipments.Where(s => s.Id != shipment.Id).Sum(ShipmentWeight);
        var vehicle = await _vehicleRepo.GetByIdAsync(orgId, load.VehicleId) ?? load.Vehicle;
        if (currentWeight + ShipmentWeight(shipment) > vehicle.CapacityKg)
            throw new InvalidOperationException(
                $"Adding this shipment would exceed the vehicle's {vehicle.CapacityKg:N0} kg capacity.");

        shipment.DeliveryLoadId = loadId;
        _shipmentRepo.Update(shipment);
        await _shipmentRepo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, loadId) ?? load;
        return ToDto(reloaded);
    }

    public async Task<DeliveryLoadDto> UnassignShipmentAsync(int orgId, int loadId, int shipmentId)
    {
        var load = await _repo.GetByIdAsync(orgId, loadId)
            ?? throw new KeyNotFoundException($"DeliveryLoad {loadId} not found");
        if (load.DispatchedDate != null)
            throw new InvalidOperationException("This delivery load has already been dispatched.");

        var shipment = load.Shipments.FirstOrDefault(s => s.Id == shipmentId)
            ?? throw new KeyNotFoundException($"Shipment {shipmentId} is not on this delivery load");

        shipment.DeliveryLoadId = null;
        _shipmentRepo.Update(shipment);
        await _shipmentRepo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, loadId) ?? load;
        return ToDto(reloaded);
    }

    /// <summary>Dispatching doesn't duplicate the stock-deduction/Picked→Shipped logic — it
    /// delegates each assigned shipment to the existing ShipmentService.ShipAsync, the same
    /// "generalize, don't reimplement" reasoning already used across this module.</summary>
    public async Task<DeliveryLoadDto> DispatchAsync(int orgId, int id)
    {
        var load = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"DeliveryLoad {id} not found");
        if (load.DispatchedDate != null)
            throw new InvalidOperationException("This delivery load has already been dispatched.");
        if (!load.Shipments.Any())
            throw new InvalidOperationException("Add at least one shipment to this load before dispatching.");

        foreach (var shipment in load.Shipments.ToList())
            await _shipmentService.ShipAsync(orgId, shipment.Id);

        load.DispatchedDate = DateTime.UtcNow;
        _repo.Update(load);
        await _repo.SaveChangesAsync();

        var vehicle = await _vehicleRepo.GetByIdAsync(orgId, load.VehicleId);
        if (vehicle != null)
        {
            vehicle.Status = "InTransit";
            vehicle.CurrentWarehouseId = null;
            _vehicleRepo.Update(vehicle);
            await _vehicleRepo.SaveChangesAsync();
        }

        var reloaded = await _repo.GetByIdAsync(orgId, load.Id) ?? load;
        return ToDto(reloaded);
    }

    private static decimal ShipmentWeight(Shipment s) => s.Packages.Sum(p => p.WeightKg ?? 0);

    private static DeliveryLoadShipmentDto ToShipmentDto(Shipment s) => new()
    {
        ShipmentId = s.Id,
        ShipmentNumber = s.ShipmentNumber,
        ReferenceLabel = s.SourceType == "SalesOrder"
            ? s.SalesOrder?.OrderNumber
            : (s.DestinationWarehouse != null ? $"Transfer to {s.DestinationWarehouse.Name}" : null),
        ShipToCity = s.ShipToCity,
        TotalWeightKg = ShipmentWeight(s),
        LineCount = s.Lines.Count
    };

    private static DeliveryLoadDto ToDto(DeliveryLoad l) => new()
    {
        Id = l.Id,
        LoadNumber = l.LoadNumber,
        VehicleId = l.VehicleId,
        VehicleCode = l.Vehicle?.Code ?? string.Empty,
        VehicleName = l.Vehicle?.Name ?? string.Empty,
        VehicleCapacityKg = l.Vehicle?.CapacityKg ?? 0,
        WarehouseId = l.WarehouseId,
        WarehouseName = l.Warehouse?.Name ?? string.Empty,
        LoadDate = l.LoadDate,
        DispatchedDate = l.DispatchedDate,
        TotalWeightKg = l.Shipments.Sum(ShipmentWeight),
        Shipments = l.Shipments.Select(ToShipmentDto).ToList()
    };
}
