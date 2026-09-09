using FoodOrder.Application.DTOs.Branch;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IBranchRepository
{
    Task<IReadOnlyList<Branch>> GetByOrganizationAsync(int organizationId);
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch> CreateAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(int id);

    /// <summary>The branch auto-created when the organization was first provisioned, if any.</summary>
    Task<int?> GetDefaultBranchIdAsync(int organizationId);

    /// <summary>
    /// Per-branch orders/revenue breakdown for one organization. Explicitly scoped by
    /// organizationId (bypassing the ambient tenant filter) so this works correctly
    /// whether the caller is an org-wide admin or a SuperAdmin inspecting any org.
    /// </summary>
    Task<List<BranchReportDto>> GetBranchReportsAsync(int organizationId);
}
