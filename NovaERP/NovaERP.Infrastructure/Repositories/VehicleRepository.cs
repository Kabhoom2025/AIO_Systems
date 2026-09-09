using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly NovaErpDbContext _ctx;

    public VehicleRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Vehicle> Query() =>
        _ctx.Vehicles.Include(v => v.CurrentWarehouse).Include(v => v.DeliveryLoads);

    public Task<List<Vehicle>> GetAllByOrgAsync(int orgId) =>
        Query().Where(v => v.OrganizationId == orgId).OrderBy(v => v.Code).ToListAsync();

    public Task<Vehicle?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(v => v.Id == id && v.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Vehicles.AnyAsync(v => v.OrganizationId == orgId && v.Code == code);

    public void Add(Vehicle vehicle)    => _ctx.Vehicles.Add(vehicle);
    public void Update(Vehicle vehicle) => _ctx.Vehicles.Update(vehicle);
    public void Remove(Vehicle vehicle) => _ctx.Vehicles.Remove(vehicle);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
