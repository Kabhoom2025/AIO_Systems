using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/leaves")]
[Authorize]
public class LeaveController : ApiControllerBase
{
    private readonly ILeaveService _service;

    public LeaveController(ILeaveService service) => _service = service;

    private int RequireEmployeeId() =>
        EmployeeId ?? throw new InvalidOperationException("No employee profile linked to this account.");

    [Authorize(Policy = "leaves.view")]
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes() =>
        Ok(await _service.GetTypesAsync(OrgId));

    [Authorize(Policy = "leaves.create")]
    [HttpPost("types")]
    public async Task<IActionResult> CreateType([FromBody] CreateLeaveTypeDto dto) =>
        Ok(await _service.CreateTypeAsync(OrgId, dto));

    [Authorize(Policy = "leaves.edit")]
    [HttpPut("types/{id}")]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateLeaveTypeDto dto) =>
        Ok(await _service.UpdateTypeAsync(OrgId, id, dto));

    [Authorize(Policy = "leaves.delete")]
    [HttpDelete("types/{id}")]
    public async Task<IActionResult> DeleteType(int id)
    {
        await _service.DeleteTypeAsync(OrgId, id);
        return NoContent();
    }

    [HttpGet("balances/my")]
    public async Task<IActionResult> GetMyBalances() =>
        Ok(await _service.GetMyBalancesAsync(OrgId, RequireEmployeeId()));

    [Authorize(Policy = "leaves.view")]
    [HttpGet("balances/employee/{employeeId}")]
    public async Task<IActionResult> GetBalancesForEmployee(int employeeId) =>
        Ok(await _service.GetBalancesForEmployeeAsync(OrgId, employeeId));

    [Authorize(Policy = "leaves.edit")]
    [HttpPost("balances/allocate")]
    public async Task<IActionResult> AllocateBalances([FromBody] AllocateBalancesDto dto) =>
        Ok(await _service.AllocateBalancesAsync(OrgId, dto));

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateLeaveRequestDto dto) =>
        Ok(await _service.CreateRequestAsync(OrgId, RequireEmployeeId(), dto));

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests() =>
        Ok(await _service.GetMyRequestsAsync(OrgId, RequireEmployeeId()));

    [Authorize(Policy = "leaves.view")]
    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] string? status) =>
        Ok(await _service.GetRequestsAsync(OrgId, status));

    [Authorize(Policy = "leaves.approve")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests() =>
        Ok(await _service.GetPendingRequestsAsync(OrgId));

    [Authorize(Policy = "leaves.approve")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRequest(int id, [FromBody] ReviewDto dto) =>
        Ok(await _service.ApproveRequestAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "leaves.approve")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] ReviewDto dto) =>
        Ok(await _service.RejectRequestAsync(OrgId, id, UserId, dto));

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelRequest(int id) =>
        Ok(await _service.CancelRequestAsync(OrgId, RequireEmployeeId(), id));

    [Authorize(Policy = "leaves.view")]
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar([FromQuery] DateOnly from, [FromQuery] DateOnly to) =>
        Ok(await _service.GetCalendarAsync(OrgId, from, to));
}
