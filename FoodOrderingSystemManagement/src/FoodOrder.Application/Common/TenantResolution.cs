using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Shared.Exceptions;

namespace FoodOrder.Application.Common;

/// <summary>
/// Resolves which Branch/Organization a newly-created row should be stamped with.
/// Read-path isolation is handled automatically by AppDbContext's global query
/// filters (see AppDbContext.OnModelCreating) — this only covers writes, since
/// filters never apply to inserts.
/// </summary>
public static class TenantResolution
{
    public static async Task<int> ResolveWriteBranchIdAsync(ICurrentUserContext currentUser, IBranchRepository branchRepository)
    {
        if (currentUser.BranchId.HasValue)
            return currentUser.BranchId.Value;

        var organizationId = ResolveWriteOrganizationId(currentUser);
        return await branchRepository.GetDefaultBranchIdAsync(organizationId)
            ?? throw new AppException("No branch exists for your organization yet.", 400);
    }

    public static int ResolveWriteOrganizationId(ICurrentUserContext currentUser) =>
        currentUser.OrganizationId
            ?? throw new AppException("An organization context is required to create this record.", 400);
}
