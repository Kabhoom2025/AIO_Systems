using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly AppDbContext _ctx;
    public DeliveryRepository(AppDbContext ctx) => _ctx = ctx;

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();

    // ── Drivers ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Driver>> GetAllDriversAsync(int? organizationId)
    {
        var q = _ctx.Drivers.AsQueryable();
        if (organizationId.HasValue)
            q = q.Where(d => d.OrganizationId == organizationId.Value);
        return await q.OrderBy(d => d.Name).ToListAsync();
    }

    public Task<Driver?> GetDriverByIdAsync(int id)
        => _ctx.Drivers.FirstOrDefaultAsync(d => d.Id == id);

    public Task AddDriverAsync(Driver driver) => _ctx.Drivers.AddAsync(driver).AsTask();

    public void UpdateDriver(Driver driver) => _ctx.Drivers.Update(driver);

    // ── Delivery Orders ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DeliveryOrder>> GetAllDeliveryOrdersAsync(int? organizationId, DateTime? date = null)
    {
        var q = _ctx.DeliveryOrders
            .Include(d => d.Order)
            .Include(d => d.Driver)
            .AsQueryable();

        if (organizationId.HasValue)
            q = q.Where(d => d.Order != null && _ctx.Orders
                .Where(o => o.Id == d.OrderId)
                .Any());

        if (date.HasValue)
            q = q.Where(d => d.CreatedDate.Date == date.Value.Date);

        return await q.OrderByDescending(d => d.CreatedDate).ToListAsync();
    }

    public Task<DeliveryOrder?> GetDeliveryOrderByIdAsync(int id)
        => _ctx.DeliveryOrders
            .Include(d => d.Order)
            .Include(d => d.Driver)
            .FirstOrDefaultAsync(d => d.Id == id);

    public Task<DeliveryOrder?> GetDeliveryOrderByOrderIdAsync(int orderId)
        => _ctx.DeliveryOrders
            .Include(d => d.Driver)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

    public Task AddDeliveryOrderAsync(DeliveryOrder delivery)
        => _ctx.DeliveryOrders.AddAsync(delivery).AsTask();

    // ── Charge Slabs ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DeliveryChargeSlab>> GetChargeSlabsAsync(int? organizationId)
    {
        var q = _ctx.DeliveryChargeSlabs.AsQueryable();
        if (organizationId.HasValue)
            q = q.Where(s => s.OrganizationId == organizationId.Value);
        return await q.OrderBy(s => s.FromKm).ToListAsync();
    }

    public Task<DeliveryChargeSlab?> GetChargeSlabByIdAsync(int id)
        => _ctx.DeliveryChargeSlabs.FindAsync(id).AsTask();

    public Task AddChargeSlabAsync(DeliveryChargeSlab slab)
        => _ctx.DeliveryChargeSlabs.AddAsync(slab).AsTask();

    public void DeleteChargeSlab(DeliveryChargeSlab slab)
        => _ctx.DeliveryChargeSlabs.Remove(slab);

    // ── Third Party Configs ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<ThirdPartyDeliveryConfig>> GetThirdPartyConfigsAsync(int? organizationId)
    {
        var q = _ctx.ThirdPartyDeliveryConfigs.AsQueryable();
        if (organizationId.HasValue)
            q = q.Where(c => c.OrganizationId == organizationId.Value);
        return await q.OrderBy(c => c.Provider).ToListAsync();
    }

    public Task<ThirdPartyDeliveryConfig?> GetThirdPartyConfigByProviderAsync(string provider, int? organizationId)
        => _ctx.ThirdPartyDeliveryConfigs
            .FirstOrDefaultAsync(c => c.Provider == provider
                && (organizationId == null ? c.OrganizationId == null : c.OrganizationId == organizationId));

    public Task AddThirdPartyConfigAsync(ThirdPartyDeliveryConfig config)
        => _ctx.ThirdPartyDeliveryConfigs.AddAsync(config).AsTask();
}
