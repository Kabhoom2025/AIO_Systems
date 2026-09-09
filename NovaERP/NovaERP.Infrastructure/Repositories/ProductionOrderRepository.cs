using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ProductionOrderRepository : IProductionOrderRepository
{
    private readonly NovaErpDbContext _ctx;

    public ProductionOrderRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<ProductionOrder> Query() =>
        _ctx.ProductionOrders
            .Include(o => o.Product)
            .Include(o => o.Warehouse)
            .Include(o => o.Owner);

    public Task<List<ProductionOrder>> GetAllByOrgAsync(int orgId) =>
        Query().Where(o => o.OrganizationId == orgId).OrderByDescending(o => o.OrderDate).ToListAsync();

    public Task<ProductionOrder?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(o => o.Id == id && o.OrganizationId == orgId);

    public void Add(ProductionOrder order)    => _ctx.ProductionOrders.Add(order);
    public void Update(ProductionOrder order) => _ctx.ProductionOrders.Update(order);
    public void Remove(ProductionOrder order) => _ctx.ProductionOrders.Remove(order);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
