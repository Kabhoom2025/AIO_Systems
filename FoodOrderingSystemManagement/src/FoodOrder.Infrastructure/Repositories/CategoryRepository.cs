using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetAllActiveAsync() =>
        await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .ToListAsync();

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null) =>
        await _context.Categories
            .AnyAsync(c =>
                c.CategoryName.ToLower() == name.ToLower().Trim() &&
                (excludeId == null || c.Id != excludeId));

    // Anonymous public menu has no JWT/tenant claims to drive the ambient query
    // filter, so it must bypass it and filter explicitly by the branchId the
    // caller provided instead.
    public async Task<IReadOnlyList<Category>> GetAllActiveByBranchAsync(int branchId) =>
        await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.BranchId == branchId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .ToListAsync();
}
