using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly PharmacyDbContext _ctx;

    public DeliveryRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Delivery>> GetAllByOrgAsync(int orgId) =>
        _ctx.Deliveries
            .Include(d => d.Branch)
            .Include(d => d.Sale)
            .Include(d => d.Customer)
            .Include(d => d.DeliveryStaff)
            .Where(d => d.OrganizationId == orgId)
            .OrderByDescending(d => d.ScheduledDate)
            .ToListAsync();

    public Task<Delivery?> GetByIdAsync(int id) =>
        _ctx.Deliveries
            .Include(d => d.Branch)
            .Include(d => d.Sale)
            .Include(d => d.Customer)
            .Include(d => d.DeliveryStaff)
            .FirstOrDefaultAsync(d => d.Id == id);

    public Task<Sale?> GetSaleAsync(int saleId) =>
        _ctx.Sales.FirstOrDefaultAsync(s => s.Id == saleId);

    public void Add(Delivery delivery) => _ctx.Deliveries.Add(delivery);
    public Task SaveChangesAsync()     => _ctx.SaveChangesAsync();
}
