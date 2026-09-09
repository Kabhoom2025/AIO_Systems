using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly NovaErpDbContext _ctx;

    public ShipmentRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Shipment> Query() =>
        _ctx.Shipments
            .Include(s => s.Warehouse)
            .Include(s => s.DestinationWarehouse)
            .Include(s => s.SalesOrder)
            .Include(s => s.Owner)
            .Include(s => s.Lines).ThenInclude(l => l.Product)
            .Include(s => s.Packages).ThenInclude(p => p.Items).ThenInclude(i => i.Product);

    public Task<List<Shipment>> GetAllByOrgAsync(int orgId) =>
        Query().Where(s => s.OrganizationId == orgId).OrderByDescending(s => s.ShipDate).ToListAsync();

    public Task<Shipment?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);

    public Task<List<Shipment>> GetPickedUnassignedByWarehouseAsync(int orgId, int warehouseId) =>
        Query().Where(s => s.OrganizationId == orgId && s.WarehouseId == warehouseId
                            && s.Status == "Picked" && s.DeliveryLoadId == null)
            .OrderBy(s => s.ShipDate)
            .ToListAsync();

    public void Add(Shipment shipment)    => _ctx.Shipments.Add(shipment);
    public void Update(Shipment shipment) => _ctx.Shipments.Update(shipment);
    public void Remove(Shipment shipment) => _ctx.Shipments.Remove(shipment);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
