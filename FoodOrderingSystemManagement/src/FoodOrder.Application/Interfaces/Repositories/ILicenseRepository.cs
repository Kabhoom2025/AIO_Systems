using FoodOrder.Application.DTOs;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ILicenseRepository
{
    Task<IEnumerable<LicenseDto>> GetAllAsync();
    Task<License?> GetByOrgAsync(int orgId);
    Task<License> UpsertAsync(int orgId, UpsertLicenseRequest request);
    Task DeleteAsync(int orgId);
}
