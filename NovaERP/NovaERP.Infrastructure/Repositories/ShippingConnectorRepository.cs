using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ShippingConnectorRepository : IShippingConnectorRepository
{
    private readonly NovaErpDbContext _ctx;

    public ShippingConnectorRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<ShippingConnector> Query() =>
        _ctx.ShippingConnectors.Include(c => c.FieldMappings);

    public Task<List<ShippingConnector>> GetAllByOrgAsync(int orgId) =>
        Query().Where(c => c.OrganizationId == orgId).OrderBy(c => c.Name).ToListAsync();

    public Task<List<ShippingConnector>> GetActiveByOrgAsync(int orgId) =>
        Query().Where(c => c.OrganizationId == orgId && c.IsActive).OrderBy(c => c.Name).ToListAsync();

    public Task<ShippingConnector?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public void Add(ShippingConnector connector) => _ctx.ShippingConnectors.Add(connector);
    public void Update(ShippingConnector connector) => _ctx.ShippingConnectors.Update(connector);
    public void Remove(ShippingConnector connector) => _ctx.ShippingConnectors.Remove(connector);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
