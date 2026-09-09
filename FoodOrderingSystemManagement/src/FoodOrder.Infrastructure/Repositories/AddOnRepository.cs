using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class AddOnRepository : IAddOnRepository
{
    private readonly AppDbContext _context;

    public AddOnRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AddOn>> GetAllAsync() =>
        await _context.AddOns
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();

    public async Task<AddOn?> GetByIdAsync(int id) =>
        await _context.AddOns.FindAsync(id);

    public async Task<IReadOnlyList<AddOn>> GetByFoodItemIdAsync(int foodItemId) =>
        await _context.ItemAddOns
            .Where(ia => ia.FoodItemId == foodItemId)
            .Select(ia => ia.AddOn)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();

    public async Task AddAsync(AddOn addOn) =>
        await _context.AddOns.AddAsync(addOn);

    public async Task UpdateAsync(AddOn addOn) =>
        _context.AddOns.Update(addOn);

    public async Task DeleteAsync(int id)
    {
        var addOn = await _context.AddOns.FindAsync(id);
        if (addOn != null) _context.AddOns.Remove(addOn);
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
