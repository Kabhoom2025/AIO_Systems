using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class FoodItemRepository : GenericRepository<FoodItem>, IFoodItemRepository
{
    public FoodItemRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<FoodItem>> GetByCategoryAsync(int categoryId) =>
        await _context.FoodItems
            .Include(f => f.Category)
            .Where(f => f.CategoryId == categoryId && f.IsAvailable)
            .OrderBy(f => f.ItemName)
            .ToListAsync();

    public async Task<IReadOnlyList<FoodItem>> GetAllWithCategoryAsync() =>
        await _context.FoodItems
            .Include(f => f.Category)
            .OrderBy(f => f.Category.DisplayOrder)
            .ThenBy(f => f.ItemName)
            .ToListAsync();

    public async Task<FoodItem?> GetByIdWithCategoryAsync(int id) =>
        await _context.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<bool> ExistsByNameInCategoryAsync(string name, int categoryId, int? excludeId = null) =>
        await _context.FoodItems
            .AnyAsync(f =>
                f.ItemName.ToLower() == name.ToLower().Trim() &&
                f.CategoryId == categoryId &&
                (excludeId == null || f.Id != excludeId));

    public async Task<FoodItem?> GetByBarcodeAsync(string barcode) =>
        await _context.FoodItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Barcode == barcode && f.IsAvailable);

    // See CategoryRepository.GetAllActiveByBranchAsync — same reasoning for the
    // anonymous public menu.
    public async Task<IReadOnlyList<FoodItem>> GetAllWithCategoryByBranchAsync(int branchId) =>
        await _context.FoodItems
            .IgnoreQueryFilters()
            .Include(f => f.Category)
            .Where(f => f.BranchId == branchId)
            .OrderBy(f => f.Category.DisplayOrder)
            .ThenBy(f => f.ItemName)
            .ToListAsync();

    // See GetAllWithCategoryByBranchAsync — same reasoning for the anonymous public order endpoint.
    public async Task<FoodItem?> GetByIdWithCategoryForBranchAsync(int id, int branchId) =>
        await _context.FoodItems
            .IgnoreQueryFilters()
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.BranchId == branchId);
}
