using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Self-service AI Assistant — a Conversation belongs to a User, not a Role, so there
/// is no permission policy on most actions here (see IAiAssistantService's doc comment). The one
/// exception is the confirm-gated create-Service-Ticket action: SendMessage computes whether the
/// caller genuinely holds the real "service-desk.create" policy (via IAuthorizationService,
/// imperatively — identical to what a declarative [Authorize(Policy=...)] would check) and
/// passes that down as plain data, so the model is never even offered the tool otherwise;
/// ConfirmAction is additionally gated declaratively as defense-in-depth.</summary>
[Route("api/my-assistant")]
[Authorize]
public class AiAssistantController : ApiControllerBase
{
    private readonly IAiAssistantService _service;
    private readonly IAuthorizationService _authz;

    public AiAssistantController(IAiAssistantService service, IAuthorizationService authz)
    {
        _service = service;
        _authz = authz;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId, UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationDto dto)
    {
        var result = await _service.CreateAsync(OrgId, UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, UserId, id);
        return NoContent();
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
    {
        var canCreateServiceTickets = (await _authz.AuthorizeAsync(User, "service-desk.create")).Succeeded;
        return Ok(await _service.SendMessageAsync(OrgId, UserId, id, dto, canCreateServiceTickets));
    }

    [Authorize(Policy = "service-desk.create")]
    [HttpPost("{id}/messages/{messageId}/confirm-action")]
    public async Task<IActionResult> ConfirmAction(int id, int messageId) =>
        Ok(await _service.ConfirmActionAsync(OrgId, UserId, id, messageId));

    [HttpPost("{id}/messages/{messageId}/cancel-action")]
    public async Task<IActionResult> CancelAction(int id, int messageId) =>
        Ok(await _service.CancelActionAsync(OrgId, UserId, id, messageId));
}
