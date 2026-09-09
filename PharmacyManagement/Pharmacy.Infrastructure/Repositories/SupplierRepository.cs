using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly PharmacyDbContext _ctx;

    public SupplierRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Supplier>> GetAllByOrgAsync(int orgId) =>
        _ctx.Suppliers
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public Task<Supplier?> GetByIdAsync(int id) =>
        _ctx.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

    public void Add(Supplier supplier)    => _ctx.Suppliers.Add(supplier);
    public void Update(Supplier supplier) => _ctx.Suppliers.Update(supplier);
    public void Remove(Supplier supplier) => _ctx.Suppliers.Remove(supplier);
    public Task SaveChangesAsync()        => _ctx.SaveChangesAsync();
}
