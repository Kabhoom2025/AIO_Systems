using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class PosSaleRepository : IPosSaleRepository
{
    private readonly NovaErpDbContext _ctx;

    public PosSaleRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<PosSale> Query() =>
        _ctx.PosSales
            .Include(s => s.Warehouse)
            .Include(s => s.CustomerAccount)
            .Include(s => s.RevenueLedgerAccount)
            .Include(s => s.PaymentLedgerAccount)
            .Include(s => s.Owner)
            .Include(s => s.Lines).ThenInclude(l => l.Product);

    public Task<List<PosSale>> GetAllByOrgAsync(int orgId) =>
        Query().Where(s => s.OrganizationId == orgId).OrderByDescending(s => s.SaleDate).ToListAsync();

    public Task<PosSale?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);

    public void Add(PosSale sale)    => _ctx.PosSales.Add(sale);
    public void Update(PosSale sale) => _ctx.PosSales.Update(sale);
    public void Remove(PosSale sale) => _ctx.PosSales.Remove(sale);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
