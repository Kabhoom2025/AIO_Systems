using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class CustomerInvoiceRepository : ICustomerInvoiceRepository
{
    private readonly NovaErpDbContext _ctx;

    public CustomerInvoiceRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<CustomerInvoice> Query() =>
        _ctx.CustomerInvoices
            .Include(i => i.Account)
            .Include(i => i.ReceivableLedgerAccount)
            .Include(i => i.Owner)
            .Include(i => i.Lines).ThenInclude(l => l.LedgerAccount);

    public Task<List<CustomerInvoice>> GetAllByOrgAsync(int orgId) =>
        Query().Where(i => i.OrganizationId == orgId).OrderByDescending(i => i.InvoiceDate).ToListAsync();

    public Task<CustomerInvoice?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(i => i.Id == id && i.OrganizationId == orgId);

    public Task<List<CustomerInvoice>> GetByAccountIdAsync(int orgId, int accountId) =>
        Query().Where(i => i.OrganizationId == orgId && i.AccountId == accountId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

    public void Add(CustomerInvoice invoice)    => _ctx.CustomerInvoices.Add(invoice);
    public void Update(CustomerInvoice invoice) => _ctx.CustomerInvoices.Update(invoice);
    public void Remove(CustomerInvoice invoice) => _ctx.CustomerInvoices.Remove(invoice);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
