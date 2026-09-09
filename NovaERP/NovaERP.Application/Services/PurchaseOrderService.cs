using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly ITaxCodeRepository _taxCodeRepo;
    private readonly IStockMovementRepository _stockMovementRepo;
    private readonly IAutomationEngine _automationEngine;
    private readonly IValidator<CreatePurchaseOrderDto> _createValidator;
    private readonly IValidator<UpdatePurchaseOrderDto> _updateValidator;

    public PurchaseOrderService(IPurchaseOrderRepository repo, ITaxCodeRepository taxCodeRepo, IStockMovementRepository stockMovementRepo,
        IAutomationEngine automationEngine,
        IValidator<CreatePurchaseOrderDto> createValidator, IValidator<UpdatePurchaseOrderDto> updateValidator)
    {
        _repo = repo;
        _taxCodeRepo = taxCodeRepo;
        _stockMovementRepo = stockMovementRepo;
        _automationEngine = automationEngine;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<PurchaseOrderDto>> GetAllAsync(int orgId)
    {
        var orders = await _repo.GetAllByOrgAsync(orgId);
        return orders.Select(ToDto).ToList();
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");
        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(int orgId, CreatePurchaseOrderDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var order = new PurchaseOrder
        {
            OrganizationId = orgId,
            VendorId = dto.VendorId,
            RfqRequestId = dto.RfqRequestId,
            Status = "Draft",
            OrderDate = dto.OrderDate,
            OwnerId = dto.OwnerId,
            Lines = await BuildLinesAsync(orgId, dto.Lines)
        };

        _repo.Add(order);
        await _repo.SaveChangesAsync();

        // PoNumber depends on the generated Id, so it's set in a second save — same
        // two-phase scheme as SalesOrder.OrderNumber / RfqRequest.RfqNumber.
        order.PoNumber = $"PO-{order.Id:D5}";
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(int orgId, int id, UpdatePurchaseOrderDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft purchase orders can be edited.");

        order.RfqRequestId = dto.RfqRequestId;
        order.OrderDate = dto.OrderDate;
        order.OwnerId = dto.OwnerId;
        order.UpdatedDate = DateTime.UtcNow;

        // Lines are owned by the order and replaced wholesale on update.
        order.Lines.Clear();
        foreach (var line in await BuildLinesAsync(orgId, dto.Lines))
            order.Lines.Add(line);

        _repo.Update(order);
        await _repo.SaveChangesAsync();
        return ToDto(order);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft purchase orders can be deleted.");

        _repo.Remove(order);
        await _repo.SaveChangesAsync();
    }

    public async Task<PurchaseOrderDto> ConfirmAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft purchase orders can be confirmed.");

        order.Status = "Confirmed";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.PurchaseOrderConfirmed, new Dictionary<string, string>
        {
            ["EntityType"] = "PurchaseOrder",
            ["EntityId"] = order.Id.ToString(),
            ["PoNumber"] = order.PoNumber
        });

        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> ReceiveAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");

        if (order.Status != "Confirmed")
            throw new InvalidOperationException("Only confirmed purchase orders can be marked received.");

        order.Status = "Received";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        // Receiving a purchase order immediately increases stock for any line tied to a
        // Product — a "Receipt" movement per line.
        foreach (var line in order.Lines.Where(l => l.ProductId.HasValue))
        {
            _stockMovementRepo.Add(new StockMovement
            {
                OrganizationId = orgId,
                ProductId = line.ProductId!.Value,
                MovementType = "Receipt",
                Quantity = line.Quantity,
                EntityType = "PurchaseOrder",
                EntityId = order.Id,
                MovementDate = DateTime.UtcNow
            });
        }
        await _stockMovementRepo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.PurchaseOrderReceived, new Dictionary<string, string>
        {
            ["EntityType"] = "PurchaseOrder",
            ["EntityId"] = order.Id.ToString(),
            ["PoNumber"] = order.PoNumber
        });

        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> CancelAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PurchaseOrder {id} not found");

        if (order.Status is "Received" or "Cancelled")
            throw new InvalidOperationException($"This purchase order is already {order.Status.ToLowerInvariant()}.");

        order.Status = "Cancelled";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.PurchaseOrderCancelled, new Dictionary<string, string>
        {
            ["EntityType"] = "PurchaseOrder",
            ["EntityId"] = order.Id.ToString(),
            ["PoNumber"] = order.PoNumber
        });

        return ToDto(order);
    }

    /// <summary>Snapshots each line's tax rate from its referenced TaxCode's components sum
    /// at save time — identical to SalesOrderService.BuildLinesAsync.</summary>
    private async Task<List<PurchaseOrderLine>> BuildLinesAsync(int orgId, List<CreatePurchaseOrderLineDto> lineDtos)
    {
        var lines = new List<PurchaseOrderLine>();
        foreach (var l in lineDtos)
        {
            decimal ratePercent = 0;
            if (l.TaxCodeId.HasValue)
            {
                var taxCode = await _taxCodeRepo.GetByIdAsync(orgId, l.TaxCodeId.Value)
                    ?? throw new KeyNotFoundException($"TaxCode {l.TaxCodeId} not found");
                ratePercent = taxCode.Components.Sum(c => c.RatePercent);
            }

            lines.Add(new PurchaseOrderLine
            {
                ItemName = l.ItemName,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxCodeId = l.TaxCodeId,
                TaxRatePercent = ratePercent,
                DisplayOrder = l.DisplayOrder
            });
        }
        return lines;
    }

    private static PurchaseOrderDto ToDto(PurchaseOrder o)
    {
        var lineDtos = o.Lines.OrderBy(l => l.DisplayOrder).Select(l =>
        {
            var subtotal = l.Quantity * l.UnitPrice;
            var tax = subtotal * l.TaxRatePercent / 100m;
            return new PurchaseOrderLineDto
            {
                Id = l.Id,
                ItemName = l.ItemName,
                ProductId = l.ProductId,
                ProductName = l.Product?.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxCodeId = l.TaxCodeId,
                TaxCodeName = l.TaxCode?.Name,
                TaxRatePercent = l.TaxRatePercent,
                DisplayOrder = l.DisplayOrder,
                LineSubtotal = subtotal,
                LineTax = tax,
                LineTotal = subtotal + tax
            };
        }).ToList();

        return new PurchaseOrderDto
        {
            Id = o.Id,
            PoNumber = o.PoNumber,
            VendorId = o.VendorId,
            VendorName = o.Vendor?.Name ?? string.Empty,
            RfqRequestId = o.RfqRequestId,
            RfqNumber = o.RfqRequest?.RfqNumber,
            Status = o.Status,
            OrderDate = o.OrderDate,
            OwnerId = o.OwnerId,
            OwnerName = o.Owner?.Name ?? string.Empty,
            Lines = lineDtos,
            Subtotal = lineDtos.Sum(l => l.LineSubtotal),
            TaxTotal = lineDtos.Sum(l => l.LineTax),
            GrandTotal = lineDtos.Sum(l => l.LineTotal)
        };
    }
}
