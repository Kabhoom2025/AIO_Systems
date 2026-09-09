using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IGoodsReceiptRepository _repo;

    public GoodsReceiptService(IGoodsReceiptRepository repo) => _repo = repo;

    public async Task<List<GoodsReceiptDto>> GetAllAsync(int orgId)
    {
        var receipts = await _repo.GetAllByOrgAsync(orgId);
        return receipts.Select(MapToDto).ToList();
    }

    public async Task<GoodsReceiptDto?> GetByIdAsync(int id)
    {
        var receipt = await _repo.GetByIdAsync(id);
        return receipt == null ? null : MapToDto(receipt);
    }

    public async Task<GoodsReceiptDto> CreateAsync(int orgId, CreateGoodsReceiptDto dto)
    {
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("A goods receipt must have at least one item.");

        var purchaseOrder = await _repo.GetPurchaseOrderWithItemsAsync(dto.PurchaseOrderId)
            ?? throw new KeyNotFoundException($"Purchase order {dto.PurchaseOrderId} not found");

        if (purchaseOrder.Status is not ("Sent" or "PartiallyReceived"))
            throw new InvalidOperationException($"Cannot receive against a purchase order with status '{purchaseOrder.Status}'.");

        var receipt = new GoodsReceipt
        {
            OrganizationId  = orgId,
            PurchaseOrderId = dto.PurchaseOrderId,
            PurchaseOrder   = purchaseOrder,
            ReceiptNumber   = $"GR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            ReceivedDate    = DateTime.UtcNow,
            ReceivedBy      = dto.ReceivedBy
        };

        foreach (var itemDto in dto.Items)
        {
            var poItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemDto.PurchaseOrderItemId)
                ?? throw new InvalidOperationException($"Purchase order item {itemDto.PurchaseOrderItemId} does not belong to purchase order {dto.PurchaseOrderId}.");

            if (poItem.ReceivedQuantity + itemDto.QuantityReceived > poItem.Quantity)
                throw new InvalidOperationException($"Received quantity for medicine {itemDto.MedicineId} exceeds the ordered quantity.");

            var expiryDate = DateTime.SpecifyKind(itemDto.ExpiryDate, DateTimeKind.Utc);
            var manufacturingDate = itemDto.ManufacturingDate.HasValue
                ? DateTime.SpecifyKind(itemDto.ManufacturingDate.Value, DateTimeKind.Utc)
                : (DateTime?)null;

            receipt.Items.Add(new GoodsReceiptItem
            {
                PurchaseOrderItemId = itemDto.PurchaseOrderItemId,
                MedicineId          = itemDto.MedicineId,
                BatchNumber         = itemDto.BatchNumber,
                ExpiryDate          = expiryDate,
                ManufacturingDate   = manufacturingDate,
                QuantityReceived    = itemDto.QuantityReceived,
                PurchasePrice       = itemDto.PurchasePrice
            });

            _repo.AddBatch(new MedicineBatch
            {
                MedicineId        = itemDto.MedicineId,
                BatchNumber       = itemDto.BatchNumber,
                ExpiryDate        = expiryDate,
                ManufacturingDate = manufacturingDate,
                QuantityReceived  = itemDto.QuantityReceived,
                CurrentQuantity   = itemDto.QuantityReceived,
                PurchasePrice     = itemDto.PurchasePrice
            });

            poItem.ReceivedQuantity += itemDto.QuantityReceived;
        }

        purchaseOrder.Status = purchaseOrder.Items.All(i => i.ReceivedQuantity >= i.Quantity)
            ? "Received"
            : "PartiallyReceived";
        purchaseOrder.UpdatedDate = DateTime.UtcNow;

        _repo.Add(receipt);
        await _repo.SaveChangesAsync();
        return MapToDto(receipt);
    }

    private static GoodsReceiptDto MapToDto(GoodsReceipt g) => new()
    {
        Id              = g.Id,
        PurchaseOrderId = g.PurchaseOrderId,
        PoNumber        = g.PurchaseOrder?.PoNumber ?? string.Empty,
        ReceiptNumber   = g.ReceiptNumber,
        ReceivedDate    = g.ReceivedDate,
        ReceivedBy      = g.ReceivedBy,
        Items = g.Items.Select(i => new GoodsReceiptItemDto
        {
            Id                  = i.Id,
            PurchaseOrderItemId = i.PurchaseOrderItemId,
            MedicineId          = i.MedicineId,
            MedicineName        = i.Medicine?.Name ?? string.Empty,
            BatchNumber         = i.BatchNumber,
            ExpiryDate          = i.ExpiryDate,
            ManufacturingDate   = i.ManufacturingDate,
            QuantityReceived    = i.QuantityReceived,
            PurchasePrice       = i.PurchasePrice
        }).ToList()
    };
}
