using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class SettingsRepository : GenericRepository<Settings>, ISettingsRepository
{
    public SettingsRepository(AppDbContext context) : base(context) { }

    // Relies on AppDbContext's tenant query filter to scope to the caller's org —
    // "the settings row" now means "my org's settings row."
    public async Task<Settings?> GetSettingsAsync() =>
        await _context.Settings.FirstOrDefaultAsync();

    public async Task<Settings?> GetByOrgAsync(int orgId) =>
        await _context.Settings.FirstOrDefaultAsync(s => s.OrganizationId == orgId);
}
