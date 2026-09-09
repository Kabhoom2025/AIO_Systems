using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _repo;
    private readonly IValidator<CreateVendorDto> _createValidator;
    private readonly IValidator<UpdateVendorDto> _updateValidator;

    public VendorService(IVendorRepository repo,
        IValidator<CreateVendorDto> createValidator, IValidator<UpdateVendorDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<VendorDto>> GetAllAsync(int orgId)
    {
        var vendors = await _repo.GetAllByOrgAsync(orgId);
        return vendors.Select(ToDto).ToList();
    }

    public async Task<VendorDto> GetByIdAsync(int orgId, int id)
    {
        var vendor = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vendor {id} not found");
        return ToDto(vendor);
    }

    public async Task<VendorDto> CreateAsync(int orgId, CreateVendorDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var vendor = new Vendor
        {
            OrganizationId = orgId,
            Name = dto.Name,
            Category = dto.Category,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            Address = dto.Address,
            IsActive = dto.IsActive,
            OwnerId = dto.OwnerId
        };

        _repo.Add(vendor);
        await _repo.SaveChangesAsync();
        return ToDto(vendor);
    }

    public async Task<VendorDto> UpdateAsync(int orgId, int id, UpdateVendorDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var vendor = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vendor {id} not found");

        vendor.Name = dto.Name;
        vendor.Category = dto.Category;
        vendor.ContactEmail = dto.ContactEmail;
        vendor.ContactPhone = dto.ContactPhone;
        vendor.Address = dto.Address;
        vendor.IsActive = dto.IsActive;
        vendor.OwnerId = dto.OwnerId;
        vendor.UpdatedDate = DateTime.UtcNow;

        _repo.Update(vendor);
        await _repo.SaveChangesAsync();
        return ToDto(vendor);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var vendor = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Vendor {id} not found");
        _repo.Remove(vendor);
        await _repo.SaveChangesAsync();
    }

    private static VendorDto ToDto(Vendor v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Category = v.Category,
        ContactEmail = v.ContactEmail,
        ContactPhone = v.ContactPhone,
        Address = v.Address,
        IsActive = v.IsActive,
        OwnerId = v.OwnerId,
        OwnerName = v.Owner?.Name ?? string.Empty
    };
}
