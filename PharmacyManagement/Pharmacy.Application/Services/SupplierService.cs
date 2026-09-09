using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo) => _repo = repo;

    public async Task<List<SupplierDto>> GetAllAsync(int orgId)
    {
        var suppliers = await _repo.GetAllByOrgAsync(orgId);
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _repo.GetByIdAsync(id);
        return supplier == null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(int orgId, CreateSupplierDto dto)
    {
        var supplier = new Supplier
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            ContactPerson  = dto.ContactPerson,
            Phone          = dto.Phone,
            Email          = dto.Email,
            Address        = dto.Address,
            GstNumber      = dto.GstNumber,
            PaymentTerms   = dto.PaymentTerms,
            CreditLimit    = dto.CreditLimit,
            IsActive       = true
        };
        _repo.Add(supplier);
        await _repo.SaveChangesAsync();
        return MapToDto(supplier);
    }

    public async Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto dto)
    {
        var supplier = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Supplier {id} not found");

        supplier.Name          = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Phone         = dto.Phone;
        supplier.Email         = dto.Email;
        supplier.Address       = dto.Address;
        supplier.GstNumber     = dto.GstNumber;
        supplier.PaymentTerms  = dto.PaymentTerms;
        supplier.CreditLimit   = dto.CreditLimit;
        supplier.IsActive      = dto.IsActive;
        supplier.UpdatedDate   = DateTime.UtcNow;

        _repo.Update(supplier);
        await _repo.SaveChangesAsync();
        return MapToDto(supplier);
    }

    public async Task DeleteAsync(int id)
    {
        var supplier = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Supplier {id} not found");
        _repo.Remove(supplier);
        await _repo.SaveChangesAsync();
    }

    private static SupplierDto MapToDto(Supplier s) => new()
    {
        Id            = s.Id,
        Name          = s.Name,
        ContactPerson = s.ContactPerson,
        Phone         = s.Phone,
        Email         = s.Email,
        Address       = s.Address,
        GstNumber     = s.GstNumber,
        PaymentTerms  = s.PaymentTerms,
        CreditLimit   = s.CreditLimit,
        IsActive      = s.IsActive
    };
}
