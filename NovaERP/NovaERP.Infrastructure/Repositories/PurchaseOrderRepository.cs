using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly NovaErpDbContext _ctx;

    public PurchaseOrderRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<PurchaseOrder> Query() =>
        _ctx.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.RfqRequest)
            .Include(o => o.Owner)
            .Include(o => o.Lines).ThenInclude(l => l.TaxCode)
            .Include(o => o.Lines).ThenInclude(l => l.Product);

    public Task<List<PurchaseOrder>> GetAllByOrgAsync(int orgId) =>
        Query().Where(o => o.OrganizationId == orgId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public Task<PurchaseOrder?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(o => o.Id == id && o.OrganizationId == orgId);

    public Task<List<PurchaseOrder>> GetByVendorIdAsync(int orgId, int vendorId) =>
        Query().Where(o => o.OrganizationId == orgId && o.VendorId == vendorId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public void Add(PurchaseOrder order)    => _ctx.PurchaseOrders.Add(order);
    public void Update(PurchaseOrder order) => _ctx.PurchaseOrders.Update(order);
    public void Remove(PurchaseOrder order) => _ctx.PurchaseOrders.Remove(order);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
