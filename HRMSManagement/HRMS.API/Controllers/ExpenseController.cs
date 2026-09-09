using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/expenses")]
[Authorize]
public class ExpenseController : ApiControllerBase
{
    private readonly IExpenseService _service;

    public ExpenseController(IExpenseService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseClaimDto dto)
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");

        var result = await _service.CreateAsync(OrgId, EmployeeId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");
        return Ok(await _service.GetMyClaimsAsync(OrgId, EmployeeId.Value));
    }

    [Authorize(Policy = "expenses.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status) =>
        Ok(await _service.GetAllAsync(OrgId, status));

    [Authorize(Policy = "expenses.view")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() =>
        Ok(await _service.GetSummaryAsync(OrgId));

    [Authorize(Policy = "expenses.view")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "expenses.approve")]
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewExpenseDto dto) =>
        Ok(await _service.ApproveAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "expenses.approve")]
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewExpenseDto dto) =>
        Ok(await _service.RejectAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "expenses.approve")]
    [HttpPost("{id:int}/reimburse")]
    public async Task<IActionResult> Reimburse(int id) =>
        Ok(await _service.ReimburseAsync(OrgId, id));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");

        await _service.DeleteAsync(OrgId, id, EmployeeId.Value);
        return NoContent();
    }
}
