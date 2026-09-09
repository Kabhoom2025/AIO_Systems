using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ISalesOrderService
{
    Task<List<SalesOrderDto>> GetAllAsync(int orgId);
    Task<SalesOrderDto> GetByIdAsync(int orgId, int id);
    Task<SalesOrderDto> CreateAsync(int orgId, CreateSalesOrderDto dto);
    Task<SalesOrderDto> UpdateAsync(int orgId, int id, UpdateSalesOrderDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<SalesOrderDto> ConfirmAsync(int orgId, int id);
    Task<SalesOrderDto> CancelAsync(int orgId, int id);
    Task<UserSalesSummaryDto> GetSummaryForOwnerAsync(int orgId, int ownerId);
}
