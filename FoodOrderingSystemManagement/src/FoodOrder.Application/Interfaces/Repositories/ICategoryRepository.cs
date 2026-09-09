using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>Returns active categories sorted by DisplayOrder ascending.</summary>
    Task<IReadOnlyList<Category>> GetAllActiveAsync();

    /// <summary>
    /// Checks whether a category name is already taken.
    /// Pass excludeId to skip the current record during an update check.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);

    /// <summary>Bypasses tenant query filters — for the anonymous public menu, which has no JWT claims.</summary>
    Task<IReadOnlyList<Category>> GetAllActiveByBranchAsync(int branchId);
}
