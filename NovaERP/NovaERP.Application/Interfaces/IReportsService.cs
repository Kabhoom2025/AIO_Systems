using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IReportsService
{
    Task<TrialBalanceDto> GetTrialBalanceAsync(int orgId);
    Task<StatusSummaryReportDto> GetSalesOrderSummaryAsync(int orgId);
    Task<StatusSummaryReportDto> GetPurchaseOrderSummaryAsync(int orgId);
}
