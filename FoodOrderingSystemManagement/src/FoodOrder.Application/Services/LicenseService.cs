using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;

namespace FoodOrder.Application.Services;

public class LicenseService(ILicenseRepository repo) : ILicenseService
{
    public Task<IEnumerable<LicenseDto>> GetAllAsync() => repo.GetAllAsync();

    public async Task<LicenseDto?> GetByOrgAsync(int orgId)
    {
        var license = await repo.GetByOrgAsync(orgId);
        if (license == null) return null;
        return new LicenseDto
        {
            Id             = license.Id,
            OrganizationId = license.OrganizationId,
            OrgName        = license.Organization.Name,
            Plan           = license.Plan,
            Status         = license.Status,
            ExpiryDate     = license.ExpiryDate,
            MaxUsers       = license.MaxUsers,
            Notes          = license.Notes,
            CreatedDate    = license.CreatedDate,
        };
    }

    public async Task<LicenseDto> UpsertAsync(int orgId, UpsertLicenseRequest request)
    {
        var license = await repo.UpsertAsync(orgId, request);
        return new LicenseDto
        {
            Id             = license.Id,
            OrganizationId = license.OrganizationId,
            OrgName        = license.Organization.Name,
            Plan           = license.Plan,
            Status         = license.Status,
            ExpiryDate     = license.ExpiryDate,
            MaxUsers       = license.MaxUsers,
            Notes          = license.Notes,
            CreatedDate    = license.CreatedDate,
        };
    }

    public Task DeleteAsync(int orgId) => repo.DeleteAsync(orgId);
}
