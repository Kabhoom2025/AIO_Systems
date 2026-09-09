using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repo;

    public OrganizationService(IOrganizationRepository repo) => _repo = repo;

    public async Task<OrganizationDto> GetProfileAsync(int orgId)
    {
        var org = await _repo.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException($"Organization {orgId} not found");
        return MapToDto(org);
    }

    public async Task<OrganizationDto> UpdateProfileAsync(int orgId, UpdateOrganizationDto dto)
    {
        var org = await _repo.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException($"Organization {orgId} not found");

        org.Name        = dto.Name;
        org.LegalName   = dto.LegalName;
        org.Address     = dto.Address;
        org.Phone       = dto.Phone;
        org.Email       = dto.Email;
        org.Website     = dto.Website;
        org.TaxNumber   = dto.TaxNumber;
        org.Timezone    = dto.Timezone;
        org.Currency    = dto.Currency;
        org.LogoUrl     = dto.LogoUrl;
        org.UpdatedDate = DateTime.UtcNow;

        _repo.Update(org);
        await _repo.SaveChangesAsync();
        return MapToDto(org);
    }

    private static OrganizationDto MapToDto(Organization o) => new()
    {
        Id        = o.Id,
        Name      = o.Name,
        Code      = o.Code,
        LegalName = o.LegalName,
        Address   = o.Address,
        Phone     = o.Phone,
        Email     = o.Email,
        Website   = o.Website,
        TaxNumber = o.TaxNumber,
        Timezone  = o.Timezone,
        Currency  = o.Currency,
        LogoUrl   = o.LogoUrl,
        IsActive  = o.IsActive
    };
}
