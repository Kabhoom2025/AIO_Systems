using System.Security.Claims;
using Chatbot.Application.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.API.Controllers;

public record ChatMessageResponse(Guid MessageId, Guid ConversationId, string Role, string Content, DateTime CreatedAt);

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController(IChatOrchestrationService chatOrchestrationService) : ControllerBase
{
    /// <summary>
    /// Non-streaming request/response chat endpoint. For a progressively-rendered response,
    /// connect to the /hubs/chat SignalR hub and invoke SendMessage instead.
    /// </summary>
    [HttpPost("message")]
    public async Task<ActionResult<ChatMessageResponse>> SendMessage(SendMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { success = false, message = "Message cannot be empty.", errorCode = "VALIDATION_ERROR" });
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await chatOrchestrationService.SendMessageAsync(userId, request, cancellationToken);

        return Ok(new ChatMessageResponse(
            result.AssistantMessage.Id,
            result.ConversationId,
            result.AssistantMessage.Role,
            result.AssistantMessage.Content,
            result.AssistantMessage.CreatedAt));
    }
}
