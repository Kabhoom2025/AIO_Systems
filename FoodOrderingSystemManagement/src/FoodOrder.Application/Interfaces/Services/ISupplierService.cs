using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDTO>> GetAllAsync();
    Task<SupplierDTO?> GetByIdAsync(int id);
    Task<SupplierDTO> CreateAsync(CreateSupplierRequest request);
    Task<SupplierDTO> UpdateAsync(int id, UpdateSupplierRequest request);
    Task DeleteAsync(int id);

    Task<IEnumerable<PurchaseOrderDTO>> GetPurchaseOrdersAsync(int supplierId);
    Task<PurchaseOrderDTO> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request);
    Task<PurchaseOrderDTO> UpdatePurchaseOrderStatusAsync(int orderId, UpdatePurchaseOrderStatusRequest request);

    Task<IEnumerable<SupplierPaymentDTO>> GetPaymentsAsync(int supplierId);
    Task<SupplierPaymentDTO> CreatePaymentAsync(CreateSupplierPaymentRequest request);
}
