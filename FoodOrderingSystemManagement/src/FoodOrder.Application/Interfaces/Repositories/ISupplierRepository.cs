using FoodOrder.Application.DTOs;
using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier> CreateAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DeleteAsync(int id);

    Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync(int supplierId);
    Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id);
    Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order);
    Task UpdatePurchaseOrderAsync(PurchaseOrder order);

    Task<IEnumerable<SupplierPayment>> GetPaymentsAsync(int supplierId);
    Task<SupplierPayment> CreatePaymentAsync(SupplierPayment payment);

    Task<string> GeneratePoNumberAsync();
}
