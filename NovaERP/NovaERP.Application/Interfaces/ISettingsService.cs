using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ISettingsService
{
    Task<OrganizationSettingsDto> GetAsync(int orgId);
    Task<OrganizationSettingsDto> UpdateAsync(int orgId, UpdateOrganizationSettingsDto dto);
}
