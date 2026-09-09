using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _repo;
    private readonly IValidator<CreateVehicleDto> _createValidator;
    private readonly IValidator<UpdateVehicleDto> _updateValidator;

    public VehicleService(IVehicleRepository repo,
        IValidator<CreateVehicleDto> createValidator, IValidator<UpdateVehicleDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<VehicleDto>> GetAllAsync(int orgId)
    {
        var vehicles = await _repo.GetAllByOrgAsync(orgId);
        return vehicles.Select(ToDto).ToList();
    }

    public async Task<VehicleDto> GetByIdAsync(int orgId, int id)
    {
        var vehicle = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found");
        return ToDto(vehicle);
    }

    public async Task<VehicleDto> CreateAsync(int orgId, CreateVehicleDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Vehicle code {code} already exists");

        var vehicle = new Vehicle
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            Model = dto.Model,
            CapacityKg = dto.CapacityKg,
            CurrentWarehouseId = dto.CurrentWarehouseId,
            IsActive = dto.IsActive,
            Status = "Available"
        };

        _repo.Add(vehicle);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, vehicle.Id) ?? vehicle;
        return ToDto(reloaded);
    }

    public async Task<VehicleDto> UpdateAsync(int orgId, int id, UpdateVehicleDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var vehicle = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found");

        vehicle.Name = dto.Name;
        vehicle.Model = dto.Model;
        vehicle.CapacityKg = dto.CapacityKg;
        vehicle.CurrentWarehouseId = dto.CurrentWarehouseId;
        vehicle.IsActive = dto.IsActive;
        vehicle.UpdatedDate = DateTime.UtcNow;

        _repo.Update(vehicle);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, vehicle.Id) ?? vehicle;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var vehicle = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found");

        if (vehicle.DeliveryLoads.Any(l => l.DispatchedDate == null))
            throw new InvalidOperationException("This vehicle has a delivery load in progress and cannot be deleted.");

        _repo.Remove(vehicle);
        await _repo.SaveChangesAsync();
    }

    private static VehicleDto ToDto(Vehicle v) => new()
    {
        Id = v.Id,
        Code = v.Code,
        Name = v.Name,
        Model = v.Model,
        CapacityKg = v.CapacityKg,
        Status = v.Status,
        CurrentWarehouseId = v.CurrentWarehouseId,
        CurrentWarehouseName = v.CurrentWarehouse?.Name,
        IsActive = v.IsActive
    };
}
