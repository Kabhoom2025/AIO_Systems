using LinkShield.Application.DTOs.Dashboard;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[AllowAnonymous]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct) =>
        Ok(await _dashboardService.GetSummaryAsync(ct));

    [HttpGet("trends")]
    public async Task<ActionResult<DashboardTrendsDto>> GetTrends([FromQuery] int days = 14, CancellationToken ct = default) =>
        Ok(await _dashboardService.GetTrendsAsync(days, ct));
}
