using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly NovaErpDbContext _ctx;

    public WarehouseRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Warehouse>> GetAllByOrgAsync(int orgId) =>
        _ctx.Warehouses
            .Include(w => w.Branch)
            .Where(w => w.OrganizationId == orgId)
            .OrderBy(w => w.Name)
            .ToListAsync();

    public Task<Warehouse?> GetByIdAsync(int orgId, int id) =>
        _ctx.Warehouses
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.Id == id && w.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Warehouses.AnyAsync(w => w.OrganizationId == orgId && w.Code == code);

    public void Add(Warehouse warehouse)    => _ctx.Warehouses.Add(warehouse);
    public void Update(Warehouse warehouse) => _ctx.Warehouses.Update(warehouse);
    public void Remove(Warehouse warehouse) => _ctx.Warehouses.Remove(warehouse);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
