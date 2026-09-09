using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly NovaErpDbContext _ctx;

    public StoreRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Store> Query() =>
        _ctx.Stores.Include(s => s.Branch).Include(s => s.Warehouse);

    public Task<List<Store>> GetAllByOrgAsync(int orgId) =>
        Query().Where(s => s.OrganizationId == orgId).OrderBy(s => s.Code).ToListAsync();

    public Task<Store?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Stores.AnyAsync(s => s.OrganizationId == orgId && s.Code == code);

    public void Add(Store store)    => _ctx.Stores.Add(store);
    public void Update(Store store) => _ctx.Stores.Update(store);
    public void Remove(Store store) => _ctx.Stores.Remove(store);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
