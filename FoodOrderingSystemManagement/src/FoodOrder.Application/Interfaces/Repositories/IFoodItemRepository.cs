using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IFoodItemRepository : IGenericRepository<FoodItem>
{
    /// <summary>Returns available food items for a given category, with Category loaded.</summary>
    Task<IReadOnlyList<FoodItem>> GetByCategoryAsync(int categoryId);

    /// <summary>Returns all food items with their Category navigation property loaded.</summary>
    Task<IReadOnlyList<FoodItem>> GetAllWithCategoryAsync();

    /// <summary>Returns a single food item with its Category navigation property loaded.</summary>
    Task<FoodItem?> GetByIdWithCategoryAsync(int id);

    /// <summary>
    /// Checks whether an item name already exists within the same category.
    /// Pass excludeId to skip the current record during an update check.
    /// </summary>
    Task<bool> ExistsByNameInCategoryAsync(string name, int categoryId, int? excludeId = null);
    Task<FoodItem?> GetByBarcodeAsync(string barcode);

    /// <summary>Bypasses tenant query filters — for the anonymous public menu, which has no JWT claims.</summary>
    Task<IReadOnlyList<FoodItem>> GetAllWithCategoryByBranchAsync(int branchId);

    /// <summary>Bypasses tenant query filters — for the anonymous public order endpoint, which has no JWT claims.</summary>
    Task<FoodItem?> GetByIdWithCategoryForBranchAsync(int id, int branchId);
}
