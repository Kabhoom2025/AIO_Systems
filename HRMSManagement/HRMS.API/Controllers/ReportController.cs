using System.Text;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/reports")]
[Authorize(Policy = "reports.view")]
public class ReportController : ApiControllerBase
{
    private readonly IReportService _service;

    public ReportController(IReportService service) => _service = service;

    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcount([FromQuery] string format = "json")
    {
        var rows = await _service.GetHeadcountReportAsync(OrgId);
        return BuildResult(rows, format, "headcount");
    }

    [HttpGet("attendance-monthly")]
    public async Task<IActionResult> GetAttendanceMonthly(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] string format = "json")
    {
        var rows = await _service.GetAttendanceMonthlyReportAsync(OrgId, year, month);
        return BuildResult(rows, format, "attendance-monthly");
    }

    [HttpGet("leave")]
    public async Task<IActionResult> GetLeave([FromQuery] int year, [FromQuery] string format = "json")
    {
        var rows = await _service.GetLeaveReportAsync(OrgId, year);
        return BuildResult(rows, format, "leave");
    }

    [HttpGet("payroll-summary")]
    public async Task<IActionResult> GetPayrollSummary([FromQuery] int year, [FromQuery] string format = "json")
    {
        var rows = await _service.GetPayrollSummaryReportAsync(OrgId, year);
        return BuildResult(rows, format, "payroll-summary");
    }

    [HttpGet("recruitment-pipeline")]
    public async Task<IActionResult> GetRecruitmentPipeline([FromQuery] string format = "json")
    {
        var rows = await _service.GetRecruitmentPipelineReportAsync(OrgId);
        return BuildResult(rows, format, "recruitment-pipeline");
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets([FromQuery] string format = "json")
    {
        var rows = await _service.GetAssetsReportAsync(OrgId);
        return BuildResult(rows, format, "assets");
    }

    private IActionResult BuildResult<T>(List<T> rows, string format, string reportName)
    {
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = ReportService.CsvBuilder.Build(rows);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"{reportName}.csv");
        }

        return Ok(rows);
    }
}
