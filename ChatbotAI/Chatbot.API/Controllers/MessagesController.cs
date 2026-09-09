using System.Security.Claims;
using Chatbot.Application.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.API.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await messageService.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }
}
