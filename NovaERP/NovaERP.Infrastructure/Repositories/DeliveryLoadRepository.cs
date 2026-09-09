using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class DeliveryLoadRepository : IDeliveryLoadRepository
{
    private readonly NovaErpDbContext _ctx;

    public DeliveryLoadRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<DeliveryLoad> Query() =>
        _ctx.DeliveryLoads
            .Include(l => l.Vehicle)
            .Include(l => l.Warehouse)
            .Include(l => l.Shipments).ThenInclude(s => s.SalesOrder)
            .Include(l => l.Shipments).ThenInclude(s => s.DestinationWarehouse)
            .Include(l => l.Shipments).ThenInclude(s => s.Lines)
            .Include(l => l.Shipments).ThenInclude(s => s.Packages);

    public Task<List<DeliveryLoad>> GetAllByOrgAsync(int orgId) =>
        Query().Where(l => l.OrganizationId == orgId).OrderByDescending(l => l.CreatedDate).ToListAsync();

    public Task<DeliveryLoad?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(l => l.Id == id && l.OrganizationId == orgId);

    public void Add(DeliveryLoad load)    => _ctx.DeliveryLoads.Add(load);
    public void Update(DeliveryLoad load) => _ctx.DeliveryLoads.Update(load);
    public void Remove(DeliveryLoad load) => _ctx.DeliveryLoads.Remove(load);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
