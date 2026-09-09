using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly NovaErpDbContext _ctx;

    public VendorRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Vendor>> GetAllByOrgAsync(int orgId) =>
        _ctx.Vendors
            .Include(v => v.Owner)
            .Where(v => v.OrganizationId == orgId)
            .OrderBy(v => v.Name)
            .ToListAsync();

    public Task<Vendor?> GetByIdAsync(int orgId, int id) =>
        _ctx.Vendors
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id && v.OrganizationId == orgId);

    public Task<Vendor?> GetByUserIdAsync(int orgId, int userId) =>
        _ctx.Vendors
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.UserId == userId && v.OrganizationId == orgId);

    public void Add(Vendor vendor)    => _ctx.Vendors.Add(vendor);
    public void Update(Vendor vendor) => _ctx.Vendors.Update(vendor);
    public void Remove(Vendor vendor) => _ctx.Vendors.Remove(vendor);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
