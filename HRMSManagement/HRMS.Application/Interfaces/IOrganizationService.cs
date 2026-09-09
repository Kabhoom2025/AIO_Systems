using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationDto> GetProfileAsync(int orgId);
    Task<OrganizationDto> UpdateProfileAsync(int orgId, UpdateOrganizationDto dto);
}
