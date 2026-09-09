using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class PlatformModuleRepository : IPlatformModuleRepository
{
    private readonly AppDbContext _db;
    public PlatformModuleRepository(AppDbContext db) => _db = db;

    public Task<List<PlatformModule>> GetAllAsync() =>
        _db.PlatformModules.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync();

    public Task<PlatformModule?> GetByIdAsync(int id) =>
        _db.PlatformModules.FindAsync(id).AsTask();

    public Task<bool> KeyExistsAsync(string key) =>
        _db.PlatformModules.AnyAsync(p => p.Key == key);

    public Task<List<string>> GetEnabledKeysForOrgAsync(int orgId) =>
        _db.OrganizationModules
           .Where(om => om.OrganizationId == orgId && om.IsEnabled)
           .Include(om => om.PlatformModule)
           .Where(om => om.PlatformModule.IsActive)
           .Select(om => om.PlatformModule.Key)
           .ToListAsync();

    public Task<List<OrganizationModule>> GetOrgModulesAsync(int orgId) =>
        _db.OrganizationModules.Where(om => om.OrganizationId == orgId).ToListAsync();

    public void Add(PlatformModule module)                    => _db.PlatformModules.Add(module);
    public void Update(PlatformModule module)                 => _db.PlatformModules.Update(module);
    public void Remove(PlatformModule module)                 => _db.PlatformModules.Remove(module);
    public void AddOrgModule(OrganizationModule om)           => _db.OrganizationModules.Add(om);
    public void RemoveOrgModules(List<OrganizationModule> oms) => _db.OrganizationModules.RemoveRange(oms);
    public Task SaveChangesAsync()                            => _db.SaveChangesAsync();
}
