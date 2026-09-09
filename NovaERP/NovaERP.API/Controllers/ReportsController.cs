using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/reports")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly IReportsService _service;

    public ReportsController(IReportsService service) => _service = service;

    [Authorize(Policy = "reports.view")]
    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance() =>
        Ok(await _service.GetTrialBalanceAsync(OrgId));

    [Authorize(Policy = "reports.view")]
    [HttpGet("sales-order-summary")]
    public async Task<IActionResult> GetSalesOrderSummary() =>
        Ok(await _service.GetSalesOrderSummaryAsync(OrgId));

    [Authorize(Policy = "reports.view")]
    [HttpGet("purchase-order-summary")]
    public async Task<IActionResult> GetPurchaseOrderSummary() =>
        Ok(await _service.GetPurchaseOrderSummaryAsync(OrgId));
}
