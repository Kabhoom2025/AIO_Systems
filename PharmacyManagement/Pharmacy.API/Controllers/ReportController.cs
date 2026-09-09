using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;

    public ReportController(IReportService service) => _service = service;

    private static DateTime AsUtc(DateTime dt) =>
        DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);

    [Authorize(Policy = "reports.view")]
    [HttpGet("profit-loss/org/{orgId}")]
    public async Task<IActionResult> ProfitLoss(int orgId, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
        Ok(await _service.GetProfitLossAsync(orgId, AsUtc(from), AsUtc(to).AddDays(1).AddTicks(-1)));

    [Authorize(Policy = "reports.view")]
    [HttpGet("gst/org/{orgId}")]
    public async Task<IActionResult> Gst(int orgId, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
        Ok(await _service.GetGstReportAsync(orgId, AsUtc(from), AsUtc(to).AddDays(1).AddTicks(-1)));

    [Authorize(Policy = "reports.view")]
    [HttpGet("cash-book/org/{orgId}")]
    public async Task<IActionResult> CashBook(int orgId, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
        Ok(await _service.GetCashBookAsync(orgId, AsUtc(from), AsUtc(to).AddDays(1).AddTicks(-1)));

    [Authorize(Policy = "reports.view")]
    [HttpGet("sales-summary/org/{orgId}")]
    public async Task<IActionResult> SalesSummary(int orgId, [FromQuery] DateTime from, [FromQuery] DateTime to) =>
        Ok(await _service.GetSalesSummaryAsync(orgId, AsUtc(from), AsUtc(to).AddDays(1).AddTicks(-1)));

    [Authorize(Policy = "reports.view")]
    [HttpGet("top-medicines/org/{orgId}")]
    public async Task<IActionResult> TopMedicines(int orgId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int take = 10) =>
        Ok(await _service.GetTopMedicinesAsync(orgId, AsUtc(from), AsUtc(to).AddDays(1).AddTicks(-1), take));

    [Authorize(Policy = "reports.view")]
    [HttpGet("inventory-valuation/org/{orgId}")]
    public async Task<IActionResult> InventoryValuation(int orgId) =>
        Ok(await _service.GetInventoryValuationAsync(orgId));
}
