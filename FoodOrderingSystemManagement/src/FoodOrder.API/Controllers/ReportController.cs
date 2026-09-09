using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ReportController : BaseApiController
{
    private readonly IReportRepository _report;
    public ReportController(IReportRepository report) => _report = report;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var data = await _report.GetSummaryStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(data));
    }

    [HttpGet("daily-sales")]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime? date)
    {
        var d = date ?? DateTime.UtcNow;
        var data = await _report.GetDailySalesAsync(d);
        return Ok(ApiResponse<object>.SuccessResult(data));
    }

    [HttpGet("top-items")]
    public async Task<IActionResult> GetTopItems([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int limit = 10)
    {
        var f = from ?? DateTime.UtcNow.AddDays(-30);
        var t = to   ?? DateTime.UtcNow;
        var data = await _report.GetTopItemsAsync(f, t, limit);
        return Ok(ApiResponse<object>.SuccessResult(data));
    }

    [HttpGet("peak-hours")]
    public async Task<IActionResult> GetPeakHours([FromQuery] DateTime? date)
    {
        var d = date ?? DateTime.UtcNow;
        var data = await _report.GetPeakHoursAsync(d);
        return Ok(ApiResponse<object>.SuccessResult(data));
    }
}
