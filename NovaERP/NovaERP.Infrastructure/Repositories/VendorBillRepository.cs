using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class VendorBillRepository : IVendorBillRepository
{
    private readonly NovaErpDbContext _ctx;

    public VendorBillRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<VendorBill> Query() =>
        _ctx.VendorBills
            .Include(b => b.Vendor)
            .Include(b => b.PayableLedgerAccount)
            .Include(b => b.Owner)
            .Include(b => b.Lines).ThenInclude(l => l.LedgerAccount);

    public Task<List<VendorBill>> GetAllByOrgAsync(int orgId) =>
        Query().Where(b => b.OrganizationId == orgId).OrderByDescending(b => b.BillDate).ToListAsync();

    public Task<VendorBill?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(b => b.Id == id && b.OrganizationId == orgId);

    public Task<List<VendorBill>> GetByVendorIdAsync(int orgId, int vendorId) =>
        Query().Where(b => b.OrganizationId == orgId && b.VendorId == vendorId)
            .OrderByDescending(b => b.BillDate)
            .ToListAsync();

    public void Add(VendorBill bill)    => _ctx.VendorBills.Add(bill);
    public void Update(VendorBill bill) => _ctx.VendorBills.Update(bill);
    public void Remove(VendorBill bill) => _ctx.VendorBills.Remove(bill);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
