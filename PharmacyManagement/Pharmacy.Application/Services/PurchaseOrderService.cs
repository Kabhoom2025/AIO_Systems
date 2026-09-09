using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private static readonly string[] AllowedStatuses = { "Draft", "Sent", "PartiallyReceived", "Received", "Cancelled" };

    private readonly IPurchaseOrderRepository _repo;

    public PurchaseOrderService(IPurchaseOrderRepository repo) => _repo = repo;

    public async Task<List<PurchaseOrderDto>> GetAllAsync(int orgId)
    {
        var orders = await _repo.GetAllByOrgAsync(orgId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(int orgId, CreatePurchaseOrderDto dto)
    {
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("A purchase order must have at least one item.");

        var order = new PurchaseOrder
        {
            OrganizationId       = orgId,
            SupplierId           = dto.SupplierId,
            PoNumber             = $"PO-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            OrderDate            = DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate.HasValue
                ? DateTime.SpecifyKind(dto.ExpectedDeliveryDate.Value, DateTimeKind.Utc)
                : null,
            Status               = "Draft",
            Notes                = dto.Notes,
            Items = dto.Items.Select(i => new PurchaseOrderItem
            {
                MedicineId = i.MedicineId,
                Quantity   = i.Quantity,
                UnitPrice  = i.UnitPrice
            }).ToList()
        };
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _repo.Add(order);
        await _repo.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusDto dto)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Purchase order {id} not found");

        if (!AllowedStatuses.Contains(dto.Status))
            throw new InvalidOperationException($"Unknown status '{dto.Status}'.");

        var validTransitions = new Dictionary<string, string[]>
        {
            ["Draft"] = new[] { "Sent", "Cancelled" },
            ["Sent"]  = new[] { "Cancelled" }
        };

        if (!validTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException($"Cannot transition purchase order from '{order.Status}' to '{dto.Status}'.");

        order.Status      = dto.Status;
        order.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Purchase order {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft purchase orders can be deleted.");

        _repo.Remove(order);
        await _repo.SaveChangesAsync();
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder p) => new()
    {
        Id                   = p.Id,
        SupplierId           = p.SupplierId,
        SupplierName         = p.Supplier?.Name ?? string.Empty,
        PoNumber             = p.PoNumber,
        OrderDate            = p.OrderDate,
        ExpectedDeliveryDate = p.ExpectedDeliveryDate,
        Status               = p.Status,
        TotalAmount          = p.TotalAmount,
        Notes                = p.Notes,
        Items = p.Items.Select(i => new PurchaseOrderItemDto
        {
            Id               = i.Id,
            MedicineId       = i.MedicineId,
            MedicineName     = i.Medicine?.Name ?? string.Empty,
            Quantity         = i.Quantity,
            UnitPrice        = i.UnitPrice,
            ReceivedQuantity = i.ReceivedQuantity
        }).ToList()
    };
}
