using FoodOrder.Application.DTOs.Employee;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/employee")]
[Authorize(Roles = "Admin")]
public class EmployeeController : BaseApiController
{
    private readonly IEmployeeService _svc;
    public EmployeeController(IEmployeeService svc) => _svc = svc;

    // ── User edit / delete ──────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _svc.UpdateUserAsync(id, dto);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"User {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, "User updated."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _svc.DeleteUserAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.SuccessResult(null, "User deleted."));
    }

    // ── Attendance ──────────────────────────────────────────────────────────

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceByDate([FromQuery] DateTime date)
    {
        var list = await _svc.GetAttendanceByDateAsync(date);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpGet("attendance/user/{userId:int}")]
    public async Task<IActionResult> GetAttendanceByUser(int userId, [FromQuery] int month, [FromQuery] int year)
    {
        var list = await _svc.GetAttendanceByUserAsync(userId, month, year);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("attendance")]
    public async Task<IActionResult> UpsertAttendance([FromBody] UpsertAttendanceRequest req)
    {
        var result = await _svc.UpsertAttendanceAsync(req);
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    // ── Salary ──────────────────────────────────────────────────────────────

    [HttpGet("salary/configs")]
    public async Task<IActionResult> GetSalaryConfigs()
    {
        var list = await _svc.GetSalaryConfigsAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPut("salary/config/{userId:int}")]
    public async Task<IActionResult> UpdateSalaryConfig(int userId, [FromBody] UpdateSalaryConfigRequest req)
    {
        var result = await _svc.UpdateSalaryConfigAsync(userId, req);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"User {userId} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpGet("salary/payments")]
    public async Task<IActionResult> GetPayments([FromQuery] int? userId = null)
    {
        var list = await _svc.GetPaymentsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("salary/payments")]
    public async Task<IActionResult> CreatePayment([FromBody] CreateSalaryPaymentRequest req)
    {
        var result = await _svc.CreatePaymentAsync(req);
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpPatch("salary/payments/{id:int}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var result = await _svc.MarkPaymentPaidAsync(id, GetCurrentUserId());
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Payment {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    // ── Shifts ──────────────────────────────────────────────────────────────

    [HttpGet("shifts")]
    public async Task<IActionResult> GetShiftDefinitions()
    {
        var list = await _svc.GetShiftDefinitionsAsync();
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("shifts")]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDefinitionRequest req)
    {
        var result = await _svc.CreateShiftDefinitionAsync(req);
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpDelete("shifts/{id:int}")]
    public async Task<IActionResult> DeleteShift(int id)
    {
        await _svc.DeleteShiftDefinitionAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null));
    }

    [HttpGet("shifts/assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var list = await _svc.GetShiftAssignmentsAsync(startDate, endDate);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("shifts/assignments")]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateShiftAssignmentRequest req)
    {
        var result = await _svc.CreateShiftAssignmentAsync(req);
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpDelete("shifts/assignments/{id:int}")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        await _svc.DeleteShiftAssignmentAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null));
    }

    // ── Performance ─────────────────────────────────────────────────────────

    [HttpGet("performance")]
    public async Task<IActionResult> GetReviews([FromQuery] int? userId = null)
    {
        var list = await _svc.GetReviewsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("performance")]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewRequest req)
    {
        var result = await _svc.CreateReviewAsync(GetCurrentUserId(), req);
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpDelete("performance/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await _svc.DeleteReviewAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null));
    }
}
