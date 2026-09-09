using FoodOrder.Application.DTOs.Branch;

namespace FoodOrder.Application.Interfaces.Services;

public interface IBranchService
{
    /// <summary>Lists branches for the caller's organization (SuperAdmin may pass an explicit organizationId).</summary>
    Task<IReadOnlyList<BranchDto>> GetMineAsync(int? explicitOrganizationId);
    Task<BranchDto> CreateAsync(CreateBranchDto dto);
    Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto);
    Task DeleteAsync(int id);

    /// <summary>Per-branch revenue/orders breakdown for the caller's organization (SuperAdmin may pass an explicit organizationId).</summary>
    Task<List<BranchReportDto>> GetBranchReportsAsync(int? explicitOrganizationId);
}
