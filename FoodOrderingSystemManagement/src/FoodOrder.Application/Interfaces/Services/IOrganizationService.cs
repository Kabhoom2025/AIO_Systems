using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationDTO>> GetAllAsync();
    Task<OrganizationDTO?> GetByIdAsync(int id);
    Task<OrganizationDTO> CreateAsync(CreateOrganizationRequest request);
    Task<OrganizationDTO> UpdateAsync(int id, UpdateOrganizationRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<OrgUserDTO>> GetOrgUsersAsync(int organizationId);
    Task<OrgUserDTO> CreateOrgAdminAsync(int organizationId, CreateOrgAdminRequest request);
    Task<OrgReportDto> GetOrgReportAsync(int orgId);
}
