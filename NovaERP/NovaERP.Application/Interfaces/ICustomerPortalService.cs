using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ICustomerPortalService
{
    Task<MyContactDto> GetMyProfileAsync(int orgId, int userId);
    Task<List<SalesOrderDto>> GetMyOrdersAsync(int orgId, int userId);
    Task<List<CustomerInvoiceDto>> GetMyInvoicesAsync(int orgId, int userId);
}
