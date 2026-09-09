using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.Application.Services;

/// <summary>Every report here is a pure in-memory aggregation over an existing admin service's
/// own GetAllAsync result — no new repository methods, the same "read-only aggregation over
/// already-fetched DTOs" approach WidgetDataService established for Dashboard Builder. Unlike
/// that self-service module, ReportsController gates every action behind reports.view — this
/// is a real Admin/Manager-facing surface, since Trial Balance exposes Finance data.</summary>
public class ReportsService : IReportsService
{
    private readonly ILedgerAccountService _ledgerAccountService;
    private readonly ISalesOrderService _salesOrderService;
    private readonly IPurchaseOrderService _purchaseOrderService;

    public ReportsService(ILedgerAccountService ledgerAccountService,
        ISalesOrderService salesOrderService, IPurchaseOrderService purchaseOrderService)
    {
        _ledgerAccountService = ledgerAccountService;
        _salesOrderService = salesOrderService;
        _purchaseOrderService = purchaseOrderService;
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(int orgId)
    {
        var accounts = await _ledgerAccountService.GetAllAsync(orgId);

        var lines = accounts.Select(a => new TrialBalanceLineDto
        {
            Code = a.Code,
            Name = a.Name,
            Type = a.Type,
            Debit = a.Balance >= 0 ? a.Balance : 0m,
            Credit = a.Balance < 0 ? -a.Balance : 0m
        }).ToList();

        return new TrialBalanceDto
        {
            Lines = lines,
            TotalDebit = lines.Sum(l => l.Debit),
            TotalCredit = lines.Sum(l => l.Credit)
        };
    }

    public async Task<StatusSummaryReportDto> GetSalesOrderSummaryAsync(int orgId)
    {
        var orders = await _salesOrderService.GetAllAsync(orgId);
        var rows = orders.GroupBy(o => o.Status)
            .OrderBy(g => g.Key)
            .Select(g => new StatusSummaryRowDto { Status = g.Key, Count = g.Count(), Total = g.Sum(o => o.GrandTotal) })
            .ToList();

        return new StatusSummaryReportDto { Rows = rows, GrandTotal = rows.Sum(r => r.Total) };
    }

    public async Task<StatusSummaryReportDto> GetPurchaseOrderSummaryAsync(int orgId)
    {
        var orders = await _purchaseOrderService.GetAllAsync(orgId);
        var rows = orders.GroupBy(o => o.Status)
            .OrderBy(g => g.Key)
            .Select(g => new StatusSummaryRowDto { Status = g.Key, Count = g.Count(), Total = g.Sum(o => o.GrandTotal) })
            .ToList();

        return new StatusSummaryReportDto { Rows = rows, GrandTotal = rows.Sum(r => r.Total) };
    }
}
