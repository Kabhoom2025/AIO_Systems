using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Self-service endpoints scoped to the caller's own linked Employee record — no
/// hrms.view/payroll.view policy on any action here (see IEmployeePortalService's doc
/// comment for why). Just [Authorize]: any logged-in user, regardless of role/permissions,
/// can reach their own data.</summary>
[Route("api/my")]
[Authorize]
public class EmployeePortalController : ApiControllerBase
{
    private readonly IEmployeePortalService _service;

    public EmployeePortalController(IEmployeePortalService service) => _service = service;

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile() =>
        Ok(await _service.GetMyProfileAsync(OrgId, UserId));

    [HttpGet("leave-requests")]
    public async Task<IActionResult> GetMyLeaveRequests() =>
        Ok(await _service.GetMyLeaveRequestsAsync(OrgId, UserId));

    [HttpPost("leave-requests")]
    public async Task<IActionResult> CreateMyLeaveRequest([FromBody] CreateMyLeaveRequestDto dto) =>
        Ok(await _service.CreateMyLeaveRequestAsync(OrgId, UserId, dto));

    [HttpPost("leave-requests/{id}/cancel")]
    public async Task<IActionResult> CancelMyLeaveRequest(int id) =>
        Ok(await _service.CancelMyLeaveRequestAsync(OrgId, UserId, id));

    [HttpGet("payslips")]
    public async Task<IActionResult> GetMyPayslips() =>
        Ok(await _service.GetMyPayslipsAsync(OrgId, UserId));

    [HttpGet("leave-types")]
    public async Task<IActionResult> GetLeaveTypes() =>
        Ok(await _service.GetLeaveTypesAsync(OrgId));
}
