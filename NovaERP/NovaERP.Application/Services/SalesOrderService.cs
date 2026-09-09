using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly ISalesOrderRepository _repo;
    private readonly ITaxCodeRepository _taxCodeRepo;
    private readonly IAutomationEngine _automationEngine;
    private readonly IValidator<CreateSalesOrderDto> _createValidator;
    private readonly IValidator<UpdateSalesOrderDto> _updateValidator;

    public SalesOrderService(ISalesOrderRepository repo, ITaxCodeRepository taxCodeRepo,
        IAutomationEngine automationEngine,
        IValidator<CreateSalesOrderDto> createValidator, IValidator<UpdateSalesOrderDto> updateValidator)
    {
        _repo = repo;
        _taxCodeRepo = taxCodeRepo;
        _automationEngine = automationEngine;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<SalesOrderDto>> GetAllAsync(int orgId)
    {
        var orders = await _repo.GetAllByOrgAsync(orgId);
        return orders.Select(ToDto).ToList();
    }

    public async Task<SalesOrderDto> GetByIdAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"SalesOrder {id} not found");
        return ToDto(order);
    }

    public async Task<SalesOrderDto> CreateAsync(int orgId, CreateSalesOrderDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var order = new SalesOrder
        {
            OrganizationId = orgId,
            AccountId = dto.AccountId,
            OpportunityId = dto.OpportunityId,
            Status = "Draft",
            OrderDate = dto.OrderDate,
            OwnerId = dto.OwnerId,
            Lines = await BuildLinesAsync(orgId, dto.Lines)
        };

        _repo.Add(order);
        await _repo.SaveChangesAsync();

        // OrderNumber depends on the generated Id, so it's set in a second save — the
        // simplest scheme that avoids a separate per-org sequence/counter entity.
        order.OrderNumber = $"SO-{order.Id:D5}";
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<SalesOrderDto> UpdateAsync(int orgId, int id, UpdateSalesOrderDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"SalesOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft sales orders can be edited.");

        order.OpportunityId = dto.OpportunityId;
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
            ?? throw new KeyNotFoundException($"SalesOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft sales orders can be deleted.");

        _repo.Remove(order);
        await _repo.SaveChangesAsync();
    }

    public async Task<SalesOrderDto> ConfirmAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"SalesOrder {id} not found");

        if (order.Status != "Draft")
            throw new InvalidOperationException("Only draft sales orders can be confirmed.");

        order.Status = "Confirmed";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        // Confirming no longer touches stock — that now happens when a Shipment fulfilling
        // this order is actually shipped (see ShipmentService.ShipAsync), the same "deduct at
        // the real fulfillment step" principle PurchaseOrder already follows at ReceiveAsync.
        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.SalesOrderConfirmed, new Dictionary<string, string>
        {
            ["EntityType"] = "SalesOrder",
            ["EntityId"] = order.Id.ToString(),
            ["OrderNumber"] = order.OrderNumber
        });

        return ToDto(order);
    }

    public async Task<SalesOrderDto> CancelAsync(int orgId, int id)
    {
        var order = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"SalesOrder {id} not found");

        if (order.Status == "Cancelled")
            throw new InvalidOperationException("This sales order is already cancelled.");

        order.Status = "Cancelled";
        order.UpdatedDate = DateTime.UtcNow;
        _repo.Update(order);
        await _repo.SaveChangesAsync();

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.SalesOrderCancelled, new Dictionary<string, string>
        {
            ["EntityType"] = "SalesOrder",
            ["EntityId"] = order.Id.ToString(),
            ["OrderNumber"] = order.OrderNumber
        });

        return ToDto(order);
    }

    /// <summary>Every metric here is derived on read from the owner's SalesOrders — nothing is
    /// stored/cached, so it's always current with the underlying orders/lines.</summary>
    public async Task<UserSalesSummaryDto> GetSummaryForOwnerAsync(int orgId, int ownerId)
    {
        var orders = await _repo.GetByOwnerIdAsync(orgId, ownerId);
        var dtos = orders.Select(ToDto).ToList();

        var nonCancelled = dtos.Where(d => d.Status != "Cancelled").ToList();
        var totalSalesValue = nonCancelled.Sum(d => d.GrandTotal);

        return new UserSalesSummaryDto
        {
            TotalOrders = dtos.Count,
            DraftOrders = dtos.Count(d => d.Status == "Draft"),
            ConfirmedOrders = dtos.Count(d => d.Status == "Confirmed"),
            CancelledOrders = dtos.Count(d => d.Status == "Cancelled"),
            TotalSalesValue = totalSalesValue,
            AverageOrderValue = nonCancelled.Count > 0 ? totalSalesValue / nonCancelled.Count : 0,
            RecentOrders = dtos.Take(8).Select(d => new UserRecentOrderDto
            {
                Id = d.Id,
                OrderNumber = d.OrderNumber,
                AccountName = d.AccountName,
                Status = d.Status,
                OrderDate = d.OrderDate,
                GrandTotal = d.GrandTotal
            }).ToList()
        };
    }

    /// <summary>Snapshots each line's tax rate from its referenced TaxCode's components sum
    /// at save time, so later rate changes don't retroactively alter historical order totals.</summary>
    private async Task<List<SalesOrderLine>> BuildLinesAsync(int orgId, List<CreateSalesOrderLineDto> lineDtos)
    {
        var lines = new List<SalesOrderLine>();
        foreach (var l in lineDtos)
        {
            decimal ratePercent = 0;
            if (l.TaxCodeId.HasValue)
            {
                var taxCode = await _taxCodeRepo.GetByIdAsync(orgId, l.TaxCodeId.Value)
                    ?? throw new KeyNotFoundException($"TaxCode {l.TaxCodeId} not found");
                ratePercent = taxCode.Components.Sum(c => c.RatePercent);
            }

            lines.Add(new SalesOrderLine
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

    private static SalesOrderDto ToDto(SalesOrder o)
    {
        var lineDtos = o.Lines.OrderBy(l => l.DisplayOrder).Select(l =>
        {
            var subtotal = l.Quantity * l.UnitPrice;
            var tax = subtotal * l.TaxRatePercent / 100m;
            return new SalesOrderLineDto
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

        return new SalesOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            AccountId = o.AccountId,
            AccountName = o.Account?.Name ?? string.Empty,
            OpportunityId = o.OpportunityId,
            OpportunityName = o.Opportunity?.Name,
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
