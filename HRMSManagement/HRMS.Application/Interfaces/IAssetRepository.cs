using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IAssetRepository
{
    Task<List<Asset>> GetAllByOrgAsync(int orgId, string? category, string? status);
    Task<Asset?> GetByIdAsync(int id);
    Task<List<Asset>> GetAllocatedToEmployeeAsync(int orgId, int employeeId);
    Task<int> GetNextTagNumberAsync(int orgId);
    void Add(Asset asset);
    void Update(Asset asset);
    void Remove(Asset asset);
    void AddAllocation(AssetAllocation allocation);
    Task SaveChangesAsync();
}
