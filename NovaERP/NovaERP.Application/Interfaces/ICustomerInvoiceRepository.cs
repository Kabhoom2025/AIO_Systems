using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ICustomerInvoiceRepository
{
    Task<List<CustomerInvoice>> GetAllByOrgAsync(int orgId);
    Task<CustomerInvoice?> GetByIdAsync(int orgId, int id);
    Task<List<CustomerInvoice>> GetByAccountIdAsync(int orgId, int accountId);
    void Add(CustomerInvoice invoice);
    void Update(CustomerInvoice invoice);
    void Remove(CustomerInvoice invoice);
    Task SaveChangesAsync();
}
