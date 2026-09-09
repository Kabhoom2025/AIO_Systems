using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/performance")]
[Authorize]
public class PerformanceController : ApiControllerBase
{
    private readonly IPerformanceService _service;

    public PerformanceController(IPerformanceService service) => _service = service;

    // Goals
    [Authorize(Policy = "performance.view")]
    [HttpGet("goals/employee/{employeeId}")]
    public async Task<IActionResult> GetGoalsByEmployee(int employeeId) =>
        Ok(await _service.GetGoalsByEmployeeAsync(OrgId, employeeId));

    [HttpGet("goals/my")]
    public async Task<IActionResult> GetMyGoals()
    {
        if (EmployeeId is not { } employeeId)
            return BadRequest(new { message = "No employee profile is linked to this user." });

        return Ok(await _service.GetGoalsByEmployeeAsync(OrgId, employeeId));
    }

    [Authorize(Policy = "performance.create")]
    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreatePerformanceGoalDto dto) =>
        Ok(await _service.CreateGoalAsync(OrgId, dto));

    [Authorize(Policy = "performance.edit")]
    [HttpPut("goals/{id}")]
    public async Task<IActionResult> UpdateGoal(int id, [FromBody] UpdatePerformanceGoalDto dto) =>
        Ok(await _service.UpdateGoalAsync(OrgId, id, dto));

    [HttpPost("goals/{id}/progress")]
    public async Task<IActionResult> UpdateGoalProgress(int id, [FromBody] UpdateGoalProgressDto dto)
    {
        var canEdit = (User.FindFirst("permissions")?.Value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Contains("performance.edit");

        return Ok(await _service.UpdateGoalProgressAsync(OrgId, id, EmployeeId, canEdit, dto));
    }

    [Authorize(Policy = "performance.delete")]
    [HttpDelete("goals/{id}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        await _service.DeleteGoalAsync(OrgId, id);
        return NoContent();
    }

    // Reviews
    [Authorize(Policy = "performance.view")]
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] string? period, [FromQuery] string? status) =>
        Ok(await _service.GetReviewsAsync(OrgId, period, status));

    [HttpGet("reviews/my")]
    public async Task<IActionResult> GetMyReviews()
    {
        if (EmployeeId is not { } employeeId)
            return BadRequest(new { message = "No employee profile is linked to this user." });

        return Ok(await _service.GetReviewsByEmployeeAsync(OrgId, employeeId));
    }

    [Authorize(Policy = "performance.view")]
    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReview(int id) =>
        Ok(await _service.GetReviewAsync(OrgId, id));

    [Authorize(Policy = "performance.create")]
    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewDto dto) =>
        Ok(await _service.CreateReviewAsync(OrgId, dto));

    [HttpPost("reviews/{id}/self")]
    public async Task<IActionResult> SubmitSelfReview(int id, [FromBody] SelfReviewDto dto)
    {
        if (EmployeeId is not { } employeeId)
            return BadRequest(new { message = "No employee profile is linked to this user." });

        return Ok(await _service.SubmitSelfReviewAsync(OrgId, id, employeeId, dto));
    }

    [Authorize(Policy = "performance.review")]
    [HttpPost("reviews/{id}/manager")]
    public async Task<IActionResult> SubmitManagerReview(int id, [FromBody] ManagerReviewDto dto) =>
        Ok(await _service.SubmitManagerReviewAsync(OrgId, id, dto));

    [Authorize(Policy = "performance.delete")]
    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await _service.DeleteReviewAsync(OrgId, id);
        return NoContent();
    }
}
