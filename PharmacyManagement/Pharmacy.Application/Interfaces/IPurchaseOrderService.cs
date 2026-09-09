using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDto>> GetAllAsync(int orgId);
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<PurchaseOrderDto> CreateAsync(int orgId, CreatePurchaseOrderDto dto);
    Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusDto dto);
    Task DeleteAsync(int id);
}
