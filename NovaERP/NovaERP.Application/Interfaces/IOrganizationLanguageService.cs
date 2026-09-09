using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IOrganizationLanguageService
{
    Task<List<OrganizationLanguageDto>> GetAllAsync(int orgId);
    Task<List<OrganizationLanguageDto>> UpdateAsync(int orgId, UpdateOrganizationLanguagesDto dto);
}
