using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/dashboard")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() =>
        Ok(await _service.GetSummaryAsync(OrgId));

    [HttpGet("my")]
    public async Task<IActionResult> GetMyDashboard() =>
        Ok(await _service.GetMyDashboardAsync(OrgId, EmployeeId));
}
