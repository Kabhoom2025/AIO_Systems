using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly PharmacyDbContext _ctx;

    public PurchaseOrderRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<PurchaseOrder>> GetAllByOrgAsync(int orgId) =>
        _ctx.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Where(p => p.OrganizationId == orgId)
            .OrderByDescending(p => p.OrderDate)
            .ToListAsync();

    public Task<PurchaseOrder?> GetByIdAsync(int id) =>
        _ctx.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);

    public void Add(PurchaseOrder purchaseOrder)    => _ctx.PurchaseOrders.Add(purchaseOrder);
    public void Remove(PurchaseOrder purchaseOrder) => _ctx.PurchaseOrders.Remove(purchaseOrder);
    public Task SaveChangesAsync()                  => _ctx.SaveChangesAsync();
}
