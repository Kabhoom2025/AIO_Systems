using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ICustomerInvoiceService
{
    Task<List<CustomerInvoiceDto>> GetAllAsync(int orgId);
    Task<CustomerInvoiceDto> GetByIdAsync(int orgId, int id);
    Task<CustomerInvoiceDto> CreateAsync(int orgId, CreateCustomerInvoiceDto dto);
    Task<CustomerInvoiceDto> UpdateAsync(int orgId, int id, UpdateCustomerInvoiceDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<CustomerInvoiceDto> SendAsync(int orgId, int id);
    Task<CustomerInvoiceDto> ReceivePaymentAsync(int orgId, int id, ReceiveCustomerInvoicePaymentDto dto);
    Task<CustomerInvoiceDto> CancelAsync(int orgId, int id);
}
