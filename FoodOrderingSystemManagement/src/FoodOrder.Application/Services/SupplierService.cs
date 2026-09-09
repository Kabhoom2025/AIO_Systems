using AutoMapper;
using FoodOrder.Application.Common;
using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces;
using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;

namespace FoodOrder.Application.Services;

public class SupplierService(ISupplierRepository repo, ICurrentUserContext currentUser, IMapper mapper) : ISupplierService
{
    public async Task<IEnumerable<SupplierDTO>> GetAllAsync()
    {
        var suppliers = await repo.GetAllAsync();
        return suppliers.Select(s => new SupplierDTO
        {
            Id = s.Id,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            TaxNumber = s.TaxNumber,
            PaymentTerms = s.PaymentTerms,
            IsActive = s.IsActive,
            Notes = s.Notes,
            PurchaseOrderCount = s.PurchaseOrders.Count,
            TotalPaid = s.Payments.Sum(p => p.Amount),
        });
    }

    public async Task<SupplierDTO?> GetByIdAsync(int id)
    {
        var s = await repo.GetByIdAsync(id);
        if (s == null) return null;
        return new SupplierDTO
        {
            Id = s.Id, Name = s.Name, ContactPerson = s.ContactPerson,
            Email = s.Email, Phone = s.Phone, Address = s.Address,
            TaxNumber = s.TaxNumber, PaymentTerms = s.PaymentTerms,
            IsActive = s.IsActive, Notes = s.Notes,
            PurchaseOrderCount = s.PurchaseOrders.Count,
            TotalPaid = s.Payments.Sum(p => p.Amount),
        };
    }

    public async Task<SupplierDTO> CreateAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            OrganizationId = TenantResolution.ResolveWriteOrganizationId(currentUser),
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            PaymentTerms = request.PaymentTerms,
            Notes = request.Notes,
            IsActive = true,
        };
        var created = await repo.CreateAsync(supplier);
        return await GetByIdAsync(created.Id) ?? throw new Exception("Supplier not found after create");
    }

    public async Task<SupplierDTO> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await repo.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Supplier {id} not found");
        supplier.Name = request.Name;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Address = request.Address;
        supplier.TaxNumber = request.TaxNumber;
        supplier.PaymentTerms = request.PaymentTerms;
        supplier.Notes = request.Notes;
        supplier.IsActive = request.IsActive;
        await repo.UpdateAsync(supplier);
        return await GetByIdAsync(id) ?? throw new Exception("Supplier not found after update");
    }

    public async Task DeleteAsync(int id) => await repo.DeleteAsync(id);

    public async Task<IEnumerable<PurchaseOrderDTO>> GetPurchaseOrdersAsync(int supplierId)
    {
        var orders = await repo.GetPurchaseOrdersAsync(supplierId);
        return orders.Select(MapOrder);
    }

    public async Task<PurchaseOrderDTO> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request)
    {
        var poNumber = await repo.GeneratePoNumberAsync();
        var order = new PurchaseOrder
        {
            OrganizationId = TenantResolution.ResolveWriteOrganizationId(currentUser),
            PoNumber = poNumber,
            SupplierId = request.SupplierId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate.HasValue
                ? DateTime.SpecifyKind(request.ExpectedDate.Value, DateTimeKind.Utc)
                : null,
            Notes = request.Notes,
            Items = request.Items.Select(i => new PurchaseOrderItem
            {
                InventoryItemId = i.InventoryItemId,
                ItemName = i.ItemName,
                Unit = i.Unit,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice,
            }).ToList(),
        };
        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);
        var created = await repo.CreatePurchaseOrderAsync(order);
        var full = await repo.GetPurchaseOrderByIdAsync(created.Id);
        return MapOrder(full!);
    }

    public async Task<PurchaseOrderDTO> UpdatePurchaseOrderStatusAsync(int orderId, UpdatePurchaseOrderStatusRequest request)
    {
        var order = await repo.GetPurchaseOrderByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"PurchaseOrder {orderId} not found");
        if (Enum.TryParse<PurchaseOrderStatus>(request.Status, out var status))
            order.Status = status;
        if (request.ReceivedDate.HasValue)
            order.ReceivedDate = DateTime.SpecifyKind(request.ReceivedDate.Value, DateTimeKind.Utc);
        if (request.Notes != null)
            order.Notes = request.Notes;
        await repo.UpdatePurchaseOrderAsync(order);
        var full = await repo.GetPurchaseOrderByIdAsync(orderId);
        return MapOrder(full!);
    }

    public async Task<IEnumerable<SupplierPaymentDTO>> GetPaymentsAsync(int supplierId)
    {
        var payments = await repo.GetPaymentsAsync(supplierId);
        return payments.Select(MapPayment);
    }

    public async Task<SupplierPaymentDTO> CreatePaymentAsync(CreateSupplierPaymentRequest request)
    {
        var payment = new SupplierPayment
        {
            OrganizationId = TenantResolution.ResolveWriteOrganizationId(currentUser),
            SupplierId = request.SupplierId,
            PurchaseOrderId = request.PurchaseOrderId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            PaymentDate = DateTime.SpecifyKind(request.PaymentDate, DateTimeKind.Utc),
            Notes = request.Notes,
        };
        var created = await repo.CreatePaymentAsync(payment);
        var full = await repo.GetPaymentsAsync(request.SupplierId);
        return MapPayment(full.First(p => p.Id == created.Id));
    }

    private static PurchaseOrderDTO MapOrder(PurchaseOrder o) => new()
    {
        Id = o.Id, PoNumber = o.PoNumber, SupplierId = o.SupplierId,
        SupplierName = o.Supplier?.Name ?? string.Empty,
        Status = o.Status.ToString(),
        OrderDate = o.OrderDate, ExpectedDate = o.ExpectedDate, ReceivedDate = o.ReceivedDate,
        TotalAmount = o.TotalAmount, Notes = o.Notes,
        Items = o.Items.Select(i => new PurchaseOrderItemDTO
        {
            Id = i.Id, InventoryItemId = i.InventoryItemId, ItemName = i.ItemName,
            Unit = i.Unit, Quantity = i.Quantity, UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice, ReceivedQuantity = i.ReceivedQuantity,
        }).ToList(),
    };

    private static SupplierPaymentDTO MapPayment(SupplierPayment p) => new()
    {
        Id = p.Id, SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        PurchaseOrderId = p.PurchaseOrderId,
        PoNumber = p.PurchaseOrder?.PoNumber,
        Amount = p.Amount, PaymentMethod = p.PaymentMethod,
        ReferenceNumber = p.ReferenceNumber,
        PaymentDate = p.PaymentDate, Notes = p.Notes,
    };
}
