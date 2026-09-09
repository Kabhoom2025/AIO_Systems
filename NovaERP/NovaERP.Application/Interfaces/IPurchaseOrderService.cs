using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDto>> GetAllAsync(int orgId);
    Task<PurchaseOrderDto> GetByIdAsync(int orgId, int id);
    Task<PurchaseOrderDto> CreateAsync(int orgId, CreatePurchaseOrderDto dto);
    Task<PurchaseOrderDto> UpdateAsync(int orgId, int id, UpdatePurchaseOrderDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<PurchaseOrderDto> ConfirmAsync(int orgId, int id);
    Task<PurchaseOrderDto> ReceiveAsync(int orgId, int id);
    Task<PurchaseOrderDto> CancelAsync(int orgId, int id);
}
