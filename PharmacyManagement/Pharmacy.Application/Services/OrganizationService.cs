using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repo;

    public OrganizationService(IOrganizationRepository repo) => _repo = repo;

    public async Task<OrganizationDto?> GetByIdAsync(int id)
    {
        var org = await _repo.GetByIdAsync(id);
        return org == null ? null : MapToDto(org);
    }

    public async Task<PublicOrganizationDto?> GetPublicDefaultAsync()
    {
        var orgIds = await _repo.GetAllActiveIdsAsync();
        var orgId = orgIds.FirstOrDefault();
        if (orgId == 0) return null;

        var org = await _repo.GetByIdAsync(orgId);
        return org == null ? null : new PublicOrganizationDto { Id = org.Id, Name = org.Name, Phone = org.Phone };
    }

    public async Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto dto)
    {
        var org = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Organization {id} not found");

        org.Name                = dto.Name;
        org.Address             = dto.Address;
        org.Phone               = dto.Phone;
        org.Email               = dto.Email;
        org.LicenseNo           = dto.LicenseNo;
        org.Currency            = dto.Currency;
        org.GstNumber           = dto.GstNumber;
        org.InvoiceNumberPrefix = dto.InvoiceNumberPrefix;
        org.UpdatedDate         = DateTime.UtcNow;

        _repo.Update(org);
        await _repo.SaveChangesAsync();
        return MapToDto(org);
    }

    private static OrganizationDto MapToDto(Organization o) => new()
    {
        Id                  = o.Id,
        Name                = o.Name,
        Address             = o.Address,
        Phone               = o.Phone,
        Email               = o.Email,
        LicenseNo           = o.LicenseNo,
        Currency            = o.Currency,
        GstNumber           = o.GstNumber,
        InvoiceNumberPrefix = o.InvoiceNumberPrefix
    };
}
