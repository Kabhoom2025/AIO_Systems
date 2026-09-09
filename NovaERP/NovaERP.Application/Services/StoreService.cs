using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _repo;
    private readonly IValidator<CreateStoreDto> _createValidator;
    private readonly IValidator<UpdateStoreDto> _updateValidator;

    public StoreService(IStoreRepository repo,
        IValidator<CreateStoreDto> createValidator, IValidator<UpdateStoreDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<StoreDto>> GetAllAsync(int orgId)
    {
        var stores = await _repo.GetAllByOrgAsync(orgId);
        return stores.Select(ToDto).ToList();
    }

    public async Task<StoreDto> GetByIdAsync(int orgId, int id)
    {
        var store = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Store {id} not found");
        return ToDto(store);
    }

    public async Task<StoreDto> CreateAsync(int orgId, CreateStoreDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Store code {code} already exists");

        var store = new Store
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,
            Address = dto.Address,
            IsActive = dto.IsActive
        };

        _repo.Add(store);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, store.Id) ?? store;
        return ToDto(reloaded);
    }

    public async Task<StoreDto> UpdateAsync(int orgId, int id, UpdateStoreDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var store = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Store {id} not found");

        store.Name = dto.Name;
        store.BranchId = dto.BranchId;
        store.WarehouseId = dto.WarehouseId;
        store.Address = dto.Address;
        store.IsActive = dto.IsActive;
        store.UpdatedDate = DateTime.UtcNow;

        _repo.Update(store);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, store.Id) ?? store;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var store = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Store {id} not found");
        _repo.Remove(store);
        await _repo.SaveChangesAsync();
    }

    private static StoreDto ToDto(Store s) => new()
    {
        Id = s.Id,
        BranchId = s.BranchId,
        BranchName = s.Branch?.Name ?? string.Empty,
        WarehouseId = s.WarehouseId,
        WarehouseName = s.Warehouse?.Name ?? string.Empty,
        Name = s.Name,
        Code = s.Code,
        Address = s.Address,
        IsActive = s.IsActive
    };
}
