using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repo;
    private readonly IValidator<CreateWarehouseDto> _createValidator;
    private readonly IValidator<UpdateWarehouseDto> _updateValidator;

    public WarehouseService(IWarehouseRepository repo,
        IValidator<CreateWarehouseDto> createValidator, IValidator<UpdateWarehouseDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<WarehouseDto>> GetAllAsync(int orgId)
    {
        var warehouses = await _repo.GetAllByOrgAsync(orgId);
        return warehouses.Select(ToDto).ToList();
    }

    public async Task<WarehouseDto> GetByIdAsync(int orgId, int id)
    {
        var warehouse = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Warehouse {id} not found");
        return ToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(int orgId, CreateWarehouseDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Warehouse code {code} already exists");

        var warehouse = new Warehouse
        {
            OrganizationId = orgId,
            BranchId = dto.BranchId,
            Name = dto.Name,
            Code = code,
            ContactName = dto.ContactName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            TaxType = dto.TaxType,
            TaxCountry = dto.TaxCountry,
            TaxId = dto.TaxId,
            IsActive = dto.IsActive
        };

        _repo.Add(warehouse);
        await _repo.SaveChangesAsync();
        return ToDto(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(int orgId, int id, UpdateWarehouseDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var warehouse = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Warehouse {id} not found");

        warehouse.BranchId = dto.BranchId;
        warehouse.Name = dto.Name;
        warehouse.ContactName = dto.ContactName;
        warehouse.Email = dto.Email;
        warehouse.Phone = dto.Phone;
        warehouse.Address = dto.Address;
        warehouse.AddressLine2 = dto.AddressLine2;
        warehouse.City = dto.City;
        warehouse.State = dto.State;
        warehouse.PostalCode = dto.PostalCode;
        warehouse.Country = dto.Country;
        warehouse.TaxType = dto.TaxType;
        warehouse.TaxCountry = dto.TaxCountry;
        warehouse.TaxId = dto.TaxId;
        warehouse.IsActive = dto.IsActive;
        warehouse.UpdatedDate = DateTime.UtcNow;

        _repo.Update(warehouse);
        await _repo.SaveChangesAsync();
        return ToDto(warehouse);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var warehouse = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Warehouse {id} not found");
        _repo.Remove(warehouse);
        await _repo.SaveChangesAsync();
    }

    private static WarehouseDto ToDto(Warehouse w) => new()
    {
        Id = w.Id,
        BranchId = w.BranchId,
        BranchName = w.Branch?.Name ?? string.Empty,
        Name = w.Name,
        Code = w.Code,
        ContactName = w.ContactName,
        Email = w.Email,
        Phone = w.Phone,
        Address = w.Address,
        AddressLine2 = w.AddressLine2,
        City = w.City,
        State = w.State,
        PostalCode = w.PostalCode,
        Country = w.Country,
        TaxType = w.TaxType,
        TaxCountry = w.TaxCountry,
        TaxId = w.TaxId,
        IsActive = w.IsActive
    };
}
