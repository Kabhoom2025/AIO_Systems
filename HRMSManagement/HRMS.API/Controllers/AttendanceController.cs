using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/attendance")]
[Authorize]
public class AttendanceController : ApiControllerBase
{
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service) => _service = service;

    private int RequireEmployeeId() =>
        EmployeeId ?? throw new InvalidOperationException("No employee profile linked to this account.");

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto) =>
        Ok(await _service.CheckInAsync(OrgId, RequireEmployeeId(), dto));

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut() =>
        Ok(await _service.CheckOutAsync(OrgId, RequireEmployeeId()));

    [HttpGet("today")]
    public async Task<IActionResult> GetToday() =>
        Ok(await _service.GetTodayAsync(OrgId, RequireEmployeeId()));

    [Authorize(Policy = "attendance.view")]
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateOnly? date) =>
        Ok(await _service.GetDailyAsync(OrgId, date ?? DateOnly.FromDateTime(DateTime.UtcNow)));

    [Authorize(Policy = "attendance.view")]
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] int year, [FromQuery] int month) =>
        Ok(await _service.GetByEmployeeMonthAsync(OrgId, employeeId, year, month));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int year, [FromQuery] int month) =>
        Ok(await _service.GetByEmployeeMonthAsync(OrgId, RequireEmployeeId(), year, month));

    [Authorize(Policy = "attendance.view")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateOnly? date) =>
        Ok(await _service.GetSummaryAsync(OrgId, date ?? DateOnly.FromDateTime(DateTime.UtcNow)));

    [HttpPost("regularizations")]
    public async Task<IActionResult> CreateRegularization([FromBody] CreateRegularizationDto dto) =>
        Ok(await _service.CreateRegularizationAsync(OrgId, RequireEmployeeId(), dto));

    [Authorize(Policy = "attendance.approve")]
    [HttpGet("regularizations")]
    public async Task<IActionResult> GetRegularizations() =>
        Ok(await _service.GetRegularizationsAsync(OrgId));

    [HttpGet("regularizations/my")]
    public async Task<IActionResult> GetMyRegularizations() =>
        Ok(await _service.GetMyRegularizationsAsync(OrgId, RequireEmployeeId()));

    [Authorize(Policy = "attendance.approve")]
    [HttpPost("regularizations/{id}/approve")]
    public async Task<IActionResult> ApproveRegularization(int id, [FromBody] ReviewDto dto) =>
        Ok(await _service.ApproveRegularizationAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "attendance.approve")]
    [HttpPost("regularizations/{id}/reject")]
    public async Task<IActionResult> RejectRegularization(int id, [FromBody] ReviewDto dto) =>
        Ok(await _service.RejectRegularizationAsync(OrgId, id, UserId, dto));
}
