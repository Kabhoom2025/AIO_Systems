using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

/// <summary>Resolves live aggregate data for a Dashboard widget by grouping the existing admin
/// services' own GetAllAsync results in-memory — no new repository query methods needed, since
/// these are read-only aggregations over already-fetched DTOs, not a new query pattern.
/// Deliberately restricted to foundation-module data (Sales, Service Desk, POS, Projects) —
/// day-to-day operational data every role can already view — since Dashboard itself is a
/// permission-free self-service surface and must not expose sensitive Finance/HRMS/Payroll
/// data through a back door.</summary>
public class WidgetDataService : IWidgetDataService
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly IServiceTicketService _serviceTicketService;
    private readonly IPosSaleService _posSaleService;
    private readonly IProjectTaskService _projectTaskService;
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly IShipmentService _shipmentService;
    private readonly IRfqRequestService _rfqRequestService;

    public WidgetDataService(ISalesOrderService salesOrderService, IServiceTicketService serviceTicketService,
        IPosSaleService posSaleService, IProjectTaskService projectTaskService,
        IPurchaseOrderService purchaseOrderService, IShipmentService shipmentService,
        IRfqRequestService rfqRequestService)
    {
        _salesOrderService = salesOrderService;
        _serviceTicketService = serviceTicketService;
        _posSaleService = posSaleService;
        _projectTaskService = projectTaskService;
        _purchaseOrderService = purchaseOrderService;
        _shipmentService = shipmentService;
        _rfqRequestService = rfqRequestService;
    }

    public async Task<WidgetDataDto> GetDataAsync(int orgId, string widgetType) => widgetType switch
    {
        "SalesOrderStatusSummary" => await SalesOrderStatusSummaryAsync(orgId),
        "ServiceTicketStatusSummary" => await ServiceTicketStatusSummaryAsync(orgId),
        "PosSalesTotal" => await PosSalesTotalAsync(orgId),
        "ProjectTaskStatusSummary" => await ProjectTaskStatusSummaryAsync(orgId),
        "PurchaseOrderStatusSummary" => await PurchaseOrderStatusSummaryAsync(orgId),
        "ShipmentStatusSummary" => await ShipmentStatusSummaryAsync(orgId),
        "RfqStatusSummary" => await RfqStatusSummaryAsync(orgId),
        _ => throw new KeyNotFoundException($"Unknown widget type: {widgetType}")
    };

    private async Task<WidgetDataDto> SalesOrderStatusSummaryAsync(int orgId)
    {
        var orders = await _salesOrderService.GetAllAsync(orgId);
        var groups = orders.GroupBy(o => o.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }

    private async Task<WidgetDataDto> ServiceTicketStatusSummaryAsync(int orgId)
    {
        var tickets = await _serviceTicketService.GetAllAsync(orgId);
        var groups = tickets.GroupBy(t => t.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }

    private async Task<WidgetDataDto> PosSalesTotalAsync(int orgId)
    {
        var sales = await _posSaleService.GetAllAsync(orgId);
        var completed = sales.Where(s => s.Status == "Completed").ToList();
        var total = completed.Sum(s => s.TotalAmount);
        return new WidgetDataDto
        {
            Labels = new List<string> { "Completed Sales" },
            Values = new List<decimal> { total },
            Total = total
        };
    }

    private async Task<WidgetDataDto> ProjectTaskStatusSummaryAsync(int orgId)
    {
        var tasks = await _projectTaskService.GetAllAsync(orgId);
        var groups = tasks.GroupBy(t => t.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }

    private async Task<WidgetDataDto> PurchaseOrderStatusSummaryAsync(int orgId)
    {
        var orders = await _purchaseOrderService.GetAllAsync(orgId);
        var groups = orders.GroupBy(o => o.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }

    private async Task<WidgetDataDto> ShipmentStatusSummaryAsync(int orgId)
    {
        var shipments = await _shipmentService.GetAllAsync(orgId);
        var groups = shipments.GroupBy(s => s.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }

    private async Task<WidgetDataDto> RfqStatusSummaryAsync(int orgId)
    {
        var rfqs = await _rfqRequestService.GetAllAsync(orgId);
        var groups = rfqs.GroupBy(r => r.Status).OrderBy(g => g.Key).ToList();
        return new WidgetDataDto
        {
            Labels = groups.Select(g => g.Key).ToList(),
            Values = groups.Select(g => (decimal)g.Count()).ToList()
        };
    }
}
