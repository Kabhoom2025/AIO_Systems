using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class SupplierRepository(AppDbContext db) : ISupplierRepository
{
    public async Task<IEnumerable<Supplier>> GetAllAsync()
        => await db.Suppliers
            .Include(s => s.PurchaseOrders)
            .Include(s => s.Payments)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<Supplier?> GetByIdAsync(int id)
        => await db.Suppliers
            .Include(s => s.PurchaseOrders)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Supplier> CreateAsync(Supplier supplier)
    {
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        db.Suppliers.Update(supplier);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var supplier = await db.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            db.Suppliers.Remove(supplier);
            await db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersAsync(int supplierId)
        => await db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .Include(po => po.Payments)
            .Where(po => po.SupplierId == supplierId)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync();

    public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id)
        => await db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .Include(po => po.Payments)
            .FirstOrDefaultAsync(po => po.Id == id);

    public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order)
    {
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    public async Task UpdatePurchaseOrderAsync(PurchaseOrder order)
    {
        db.PurchaseOrders.Update(order);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<SupplierPayment>> GetPaymentsAsync(int supplierId)
        => await db.SupplierPayments
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseOrder)
            .Where(p => p.SupplierId == supplierId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<SupplierPayment> CreatePaymentAsync(SupplierPayment payment)
    {
        db.SupplierPayments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    public async Task<string> GeneratePoNumberAsync()
    {
        var count = await db.PurchaseOrders.CountAsync();
        return $"PO-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}
