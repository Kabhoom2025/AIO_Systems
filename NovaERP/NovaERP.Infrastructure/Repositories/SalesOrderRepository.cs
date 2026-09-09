using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly NovaErpDbContext _ctx;

    public SalesOrderRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<SalesOrder> Query() =>
        _ctx.SalesOrders
            .Include(o => o.Account)
            .Include(o => o.Opportunity)
            .Include(o => o.Owner)
            .Include(o => o.Lines).ThenInclude(l => l.TaxCode)
            .Include(o => o.Lines).ThenInclude(l => l.Product);

    public Task<List<SalesOrder>> GetAllByOrgAsync(int orgId) =>
        Query().Where(o => o.OrganizationId == orgId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public Task<SalesOrder?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(o => o.Id == id && o.OrganizationId == orgId);

    public Task<List<SalesOrder>> GetByAccountIdAsync(int orgId, int accountId) =>
        Query().Where(o => o.OrganizationId == orgId && o.AccountId == accountId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public Task<List<SalesOrder>> GetByOwnerIdAsync(int orgId, int ownerId) =>
        Query().Where(o => o.OrganizationId == orgId && o.OwnerId == ownerId)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();

    public void Add(SalesOrder order)    => _ctx.SalesOrders.Add(order);
    public void Update(SalesOrder order) => _ctx.SalesOrders.Update(order);
    public void Remove(SalesOrder order) => _ctx.SalesOrders.Remove(order);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
