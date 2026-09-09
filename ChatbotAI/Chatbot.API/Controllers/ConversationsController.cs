using System.Security.Claims;
using Chatbot.Application.Conversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.API.Controllers;

[ApiController]
[Authorize]
[Route("api/conversations")]
public class ConversationsController(IConversationService conversationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await conversationService.GetForUserAsync(GetUserId(), search, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await conversationService.GetDetailAsync(GetUserId(), id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<List<Application.Messages.MessageDto>>> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        var result = await conversationService.GetDetailAsync(GetUserId(), id, cancellationToken);
        return Ok(result.Messages);
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> Create(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await conversationService.CreateAsync(GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ConversationDto>> Update(Guid id, UpdateConversationRequest request, CancellationToken cancellationToken)
    {
        var result = await conversationService.UpdateAsync(GetUserId(), id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await conversationService.DeleteAsync(GetUserId(), id, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
