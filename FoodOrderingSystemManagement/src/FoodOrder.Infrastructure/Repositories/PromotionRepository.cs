using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _ctx;
    public PromotionRepository(AppDbContext ctx) { _ctx = ctx; }

    public async Task<IReadOnlyList<Promotion>> GetAllAsync() =>
        await _ctx.Promotions.OrderByDescending(p => p.CreatedDate).ToListAsync();

    public async Task<Promotion?> GetByIdAsync(int id) =>
        await _ctx.Promotions.FindAsync(id);

    public async Task<Promotion?> GetByCodeAsync(string code) =>
        await _ctx.Promotions.FirstOrDefaultAsync(p => p.Code == code);

    public async Task<IReadOnlyList<Promotion>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return await _ctx.Promotions
            .Where(p => p.IsActive &&
                        (p.StartDate == null || p.StartDate <= now) &&
                        (p.EndDate   == null || p.EndDate   >= now))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Promotion>> GetPublicActiveAsync(int organizationId)
    {
        var now = DateTime.UtcNow;
        return await _ctx.Promotions
            .IgnoreQueryFilters()
            .Where(p => p.OrganizationId == organizationId && p.IsActive && p.IsPublic &&
                        (p.StartDate == null || p.StartDate <= now) &&
                        (p.EndDate   == null || p.EndDate   >= now))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Promotion promotion) =>
        await _ctx.Promotions.AddAsync(promotion);

    public void Update(Promotion promotion) =>
        _ctx.Promotions.Update(promotion);

    public void Delete(Promotion promotion) =>
        _ctx.Promotions.Remove(promotion);

    public async Task SaveChangesAsync() =>
        await _ctx.SaveChangesAsync();
}
