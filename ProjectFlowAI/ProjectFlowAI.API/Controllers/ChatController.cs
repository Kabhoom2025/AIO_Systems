using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Chat;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public ChatController(IMediator mediator, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    // "Who is doing this" always comes from the JWT, never a client-supplied field — mirrors
    // WorkItemsController.CurrentUserId exactly.
    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record CreateChannelRequest(Guid OrganizationId, Guid? ProjectId, string Name, bool IsPrivate, IReadOnlyList<Guid>? MemberUserIds);
    public record AddChannelMemberRequest(Guid UserId);
    public record PostMessageRequest(string Body, Guid? ParentMessageId);
    public record CreateConversationRequest(Guid OtherUserId);
    public record ToggleReactionRequest(string Emoji);

    // --- Channels ---

    [HttpGet("channels")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<IReadOnlyList<ChatChannelDto>>> ListChannels([FromQuery] Guid organizationId, [FromQuery] Guid? projectId)
    {
        var result = await _mediator.Send(new ListChatChannelsQuery(organizationId, projectId, CurrentUserId));
        return Ok(result);
    }

    [HttpPost("channels")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<ChatChannelDto>> CreateChannel(CreateChannelRequest request)
    {
        var result = await _mediator.Send(new CreateChannelCommand(request.OrganizationId, request.ProjectId,
            request.Name, request.IsPrivate, request.MemberUserIds ?? Array.Empty<Guid>(), CurrentUserId));
        return Ok(result);
    }

    [HttpPost("channels/{id:guid}/members")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> AddMember(Guid id, AddChannelMemberRequest request)
    {
        await _mediator.Send(new AddChannelMemberCommand(id, request.UserId));
        return NoContent();
    }

    [HttpDelete("channels/{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _mediator.Send(new RemoveChannelMemberCommand(id, userId));
        return NoContent();
    }

    [HttpPost("channels/{id:guid}/read")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> MarkChannelRead(Guid id)
    {
        await _mediator.Send(new MarkChannelReadCommand(id, CurrentUserId));
        return NoContent();
    }

    // --- Channel messages ---

    [HttpGet("channels/{id:guid}/messages")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<ChatMessageListDto>> GetChannelMessages(Guid id, [FromQuery] Guid? before, [FromQuery] int limit = 50)
    {
        var result = await _mediator.Send(new GetChannelMessagesQuery(id, CurrentUserId, before, limit));
        return Ok(result);
    }

    [HttpPost("channels/{id:guid}/messages")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<ChatMessageDto>> PostChannelMessage(Guid id, PostMessageRequest request)
    {
        var result = await _mediator.Send(new PostChannelMessageCommand(id, CurrentUserId, request.Body, request.ParentMessageId));
        return Ok(result);
    }

    // --- Direct conversations ---

    [HttpGet("conversations")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<IReadOnlyList<DirectConversationDto>>> ListConversations([FromQuery] Guid organizationId)
    {
        var result = await _mediator.Send(new ListConversationsQuery(organizationId, CurrentUserId));
        return Ok(result);
    }

    [HttpPost("conversations")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<DirectConversationDto>> CreateConversation(CreateConversationRequest request)
    {
        var result = await _mediator.Send(new CreateConversationCommand(CurrentUserId, request.OtherUserId));
        return Ok(result);
    }

    [HttpGet("conversations/{id:guid}/messages")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<ChatMessageListDto>> GetConversationMessages(Guid id, [FromQuery] Guid? before, [FromQuery] int limit = 50)
    {
        var result = await _mediator.Send(new GetConversationMessagesQuery(id, CurrentUserId, before, limit));
        return Ok(result);
    }

    [HttpPost("conversations/{id:guid}/messages")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<ActionResult<ChatMessageDto>> PostConversationMessage(Guid id, PostMessageRequest request)
    {
        var result = await _mediator.Send(new PostConversationMessageCommand(id, CurrentUserId, request.Body, request.ParentMessageId));
        return Ok(result);
    }

    // --- Reactions / attachments / delete ---

    [HttpPost("messages/{id:guid}/reactions")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> ToggleReaction(Guid id, ToggleReactionRequest request)
    {
        await _mediator.Send(new ToggleMessageReactionCommand(id, CurrentUserId, request.Emoji));
        return NoContent();
    }

    [HttpPost("messages/{id:guid}/attachments")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<MessageAttachmentDto>> UploadAttachment(Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty.");

        await using var stream = file.OpenReadStream();
        var stored = await _fileStorage.SaveAsync(stream, file.FileName);

        var result = await _mediator.Send(new AddMessageAttachmentCommand(id, file.FileName, stored.RelativePath, stored.SizeBytes, CurrentUserId));
        return Ok(result);
    }

    [HttpGet("attachments/{id:guid}/download")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> DownloadAttachment(Guid id)
    {
        var file = await _mediator.Send(new GetMessageAttachmentFileQuery(id));
        var contentType = _fileStorage.GetContentType(file.FileName);
        var stream = _fileStorage.OpenRead(file.FileUrl);
        return File(stream, contentType, file.FileName);
    }

    [HttpDelete("messages/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.ChatUse)]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        await _mediator.Send(new DeleteMessageCommand(id, CurrentUserId));
        return NoContent();
    }
}
