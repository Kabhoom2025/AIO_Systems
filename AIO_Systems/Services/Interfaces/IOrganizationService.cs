using AIO_Systems.DTOs.Organization;

namespace AIO_Systems.Services.Interfaces;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationDto>> GetAllAsync();
    Task<OrganizationDto?> GetByIdAsync(int id);
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request);
    Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<OrgUserDto>> GetOrgUsersAsync(int organizationId);
    Task<OrgUserDto> CreateOrgAdminAsync(int organizationId, CreateOrgAdminRequest request);

    Task<IEnumerable<LicenseDto>> GetAllLicensesAsync();
    Task<LicenseDto?> GetLicenseByOrgAsync(int orgId);
    Task<LicenseDto> UpsertLicenseAsync(int orgId, UpsertLicenseRequest request);
    Task DeleteLicenseAsync(int orgId);
}
