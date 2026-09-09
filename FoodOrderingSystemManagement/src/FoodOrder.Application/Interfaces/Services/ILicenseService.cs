using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface ILicenseService
{
    Task<IEnumerable<LicenseDto>> GetAllAsync();
    Task<LicenseDto?> GetByOrgAsync(int orgId);
    Task<LicenseDto> UpsertAsync(int orgId, UpsertLicenseRequest request);
    Task DeleteAsync(int orgId);
}
