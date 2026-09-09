using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationDto?> GetByIdAsync(int id);
    Task<PublicOrganizationDto?> GetPublicDefaultAsync();
    Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto dto);
}
