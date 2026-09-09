using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/helpdesk")]
[Authorize]
public class HelpDeskController : ApiControllerBase
{
    private readonly IHelpDeskService _service;

    public HelpDeskController(IHelpDeskService service) => _service = service;

    /// <summary>True when the caller holds the "helpdesk.view" permission (i.e. can see all org tickets).</summary>
    private bool CanViewAll =>
        (User.FindFirst("permissions")?.Value ?? string.Empty).Contains("helpdesk.view");

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");

        var result = await _service.CreateTicketAsync(OrgId, EmployeeId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("tickets/my")]
    public async Task<IActionResult> GetMy()
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");
        return Ok(await _service.GetMyTicketsAsync(OrgId, EmployeeId.Value));
    }

    [Authorize(Policy = "helpdesk.view")]
    [HttpGet("tickets")]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? category) =>
        Ok(await _service.GetAllAsync(OrgId, status, category));

    [Authorize(Policy = "helpdesk.view")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() =>
        Ok(await _service.GetSummaryAsync(OrgId));

    [HttpGet("tickets/{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id, EmployeeId, CanViewAll));

    [HttpPost("tickets/{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto) =>
        Ok(await _service.AddCommentAsync(OrgId, id, UserId, UserName, EmployeeId, CanViewAll, dto));

    [Authorize(Policy = "helpdesk.edit")]
    [HttpPost("tickets/{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketDto dto) =>
        Ok(await _service.AssignAsync(OrgId, id, dto));

    [Authorize(Policy = "helpdesk.edit")]
    [HttpPost("tickets/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto dto) =>
        Ok(await _service.UpdateStatusAsync(OrgId, id, dto));
}
