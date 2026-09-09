using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Chat;

/// <summary>Shared helpers for the Chat feature's command handlers.</summary>
internal static class ChatAccess
{
    public static async Task EnsureChannelMemberAsync(IProjectFlowDbContext db, Guid channelId, Guid userId, CancellationToken ct)
    {
        var isMember = await db.ChatChannelMembers.AnyAsync(m => m.ChannelId == channelId && m.UserId == userId, ct);
        if (!isMember)
            throw new UnauthorizedDomainException("You are not a member of this channel.");
    }

    public static async Task<DirectConversation> EnsureConversationParticipantAsync(IProjectFlowDbContext db, Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conversation = await db.DirectConversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct)
            ?? throw new NotFoundException("DirectConversation", conversationId);
        if (conversation.UserAId != userId && conversation.UserBId != userId)
            throw new UnauthorizedDomainException("You are not a participant in this conversation.");
        return conversation;
    }

    public static async Task<ChatMessageDto> MapMessageAsync(IProjectFlowDbContext db, IMapper mapper, Guid messageId, CancellationToken ct)
    {
        var withNav = await db.ChatMessages
            .Include(m => m.AuthorUser)
            .Include(m => m.Reactions)
            .Include(m => m.Attachments)
            .Include(m => m.Replies)
            .FirstAsync(m => m.Id == messageId, ct);
        return mapper.Map<ChatMessageDto>(withNav);
    }
}

// ---------------------------------------------------------------------------
// Channels
// ---------------------------------------------------------------------------

public record CreateChannelCommand(Guid OrganizationId, Guid? ProjectId, string Name, bool IsPrivate,
    IReadOnlyList<Guid> MemberUserIds, Guid CreatedByUserId) : IRequest<ChatChannelDto>;

public class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.MemberUserIds).NotNull();
    }
}

public class CreateChannelCommandHandler : IRequestHandler<CreateChannelCommand, ChatChannelDto>
{
    private readonly IProjectFlowDbContext _db;

    public CreateChannelCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<ChatChannelDto> Handle(CreateChannelCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);

        var channel = new ChatChannel
        {
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            Name = request.Name,
            IsPrivate = request.IsPrivate,
            CreatedByUserId = request.CreatedByUserId
        };
        _db.ChatChannels.Add(channel);

        var memberIds = new HashSet<Guid>(request.MemberUserIds) { request.CreatedByUserId };
        foreach (var userId in memberIds)
            _db.ChatChannelMembers.Add(new ChatChannelMember { ChannelId = channel.Id, UserId = userId });

        await _db.SaveChangesAsync(cancellationToken);

        return new ChatChannelDto(channel.Id, channel.OrganizationId, channel.ProjectId, channel.Name,
            channel.IsPrivate, memberIds.Count, 0, channel.CreatedAt);
    }
}

public record AddChannelMemberCommand(Guid ChannelId, Guid UserId) : IRequest<Unit>;

public class AddChannelMemberCommandValidator : AbstractValidator<AddChannelMemberCommand>
{
    public AddChannelMemberCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class AddChannelMemberCommandHandler : IRequestHandler<AddChannelMemberCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public AddChannelMemberCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(AddChannelMemberCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.ChatChannels.AnyAsync(c => c.Id == request.ChannelId, cancellationToken))
            throw new NotFoundException("ChatChannel", request.ChannelId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);

        var alreadyMember = await _db.ChatChannelMembers.AnyAsync(m => m.ChannelId == request.ChannelId && m.UserId == request.UserId, cancellationToken);
        if (!alreadyMember)
        {
            _db.ChatChannelMembers.Add(new ChatChannelMember { ChannelId = request.ChannelId, UserId = request.UserId });
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record RemoveChannelMemberCommand(Guid ChannelId, Guid UserId) : IRequest<Unit>;

public class RemoveChannelMemberCommandValidator : AbstractValidator<RemoveChannelMemberCommand>
{
    public RemoveChannelMemberCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class RemoveChannelMemberCommandHandler : IRequestHandler<RemoveChannelMemberCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveChannelMemberCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveChannelMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.ChatChannelMembers.FirstOrDefaultAsync(m => m.ChannelId == request.ChannelId && m.UserId == request.UserId, cancellationToken);
        if (member != null)
        {
            _db.ChatChannelMembers.Remove(member);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record MarkChannelReadCommand(Guid ChannelId, Guid UserId) : IRequest<Unit>;

public class MarkChannelReadCommandValidator : AbstractValidator<MarkChannelReadCommand>
{
    public MarkChannelReadCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class MarkChannelReadCommandHandler : IRequestHandler<MarkChannelReadCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public MarkChannelReadCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(MarkChannelReadCommand request, CancellationToken cancellationToken)
    {
        await ChatAccess.EnsureChannelMemberAsync(_db, request.ChannelId, request.UserId, cancellationToken);

        var read = await _db.ChannelReads.FirstOrDefaultAsync(r => r.ChannelId == request.ChannelId && r.UserId == request.UserId, cancellationToken);
        if (read == null)
        {
            read = new ChannelRead { ChannelId = request.ChannelId, UserId = request.UserId };
            _db.ChannelReads.Add(read);
        }
        read.LastReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Messages (channel)
// ---------------------------------------------------------------------------

public record PostChannelMessageCommand(Guid ChannelId, Guid AuthorUserId, string Body, Guid? ParentMessageId) : IRequest<ChatMessageDto>;

public class PostChannelMessageCommandValidator : AbstractValidator<PostChannelMessageCommand>
{
    public PostChannelMessageCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.AuthorUserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

public class PostChannelMessageCommandHandler : IRequestHandler<PostChannelMessageCommand, ChatMessageDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IChatRealtimeNotifier _realtime;

    public PostChannelMessageCommandHandler(IProjectFlowDbContext db, IMapper mapper, IChatRealtimeNotifier realtime)
    {
        _db = db; _mapper = mapper; _realtime = realtime;
    }

    public async Task<ChatMessageDto> Handle(PostChannelMessageCommand request, CancellationToken cancellationToken)
    {
        await ChatAccess.EnsureChannelMemberAsync(_db, request.ChannelId, request.AuthorUserId, cancellationToken);

        if (request.ParentMessageId.HasValue && !await _db.ChatMessages.AnyAsync(m => m.Id == request.ParentMessageId.Value, cancellationToken))
            throw new NotFoundException("ChatMessage", request.ParentMessageId.Value);

        var message = new ChatMessage
        {
            ChannelId = request.ChannelId,
            AuthorUserId = request.AuthorUserId,
            Body = request.Body,
            ParentMessageId = request.ParentMessageId
        };
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await ChatAccess.MapMessageAsync(_db, _mapper, message.Id, cancellationToken);
        await _realtime.MessageReceivedAsync(request.ChannelId, null, dto, cancellationToken);
        return dto;
    }
}

// ---------------------------------------------------------------------------
// Direct conversations
// ---------------------------------------------------------------------------

public record CreateConversationCommand(Guid CurrentUserId, Guid OtherUserId) : IRequest<DirectConversationDto>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.OtherUserId).NotEmpty();
        RuleFor(x => x).Must(x => x.CurrentUserId != x.OtherUserId).WithMessage("Cannot start a conversation with yourself.");
    }
}

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, DirectConversationDto>
{
    private readonly IProjectFlowDbContext _db;

    public CreateConversationCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<DirectConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Users.AnyAsync(u => u.Id == request.OtherUserId, cancellationToken))
            throw new NotFoundException("User", request.OtherUserId);

        // Normalize: the lexicographically-smaller Guid is always stored as UserAId, so
        // CreateConversation(A, B) and CreateConversation(B, A) find the very same row instead of
        // creating a duplicate — see DirectConversationNormalizationTests.
        var (userAId, userBId) = NormalizePair(request.CurrentUserId, request.OtherUserId);

        var conversation = await _db.DirectConversations
            .FirstOrDefaultAsync(c => c.UserAId == userAId && c.UserBId == userBId, cancellationToken);

        if (conversation == null)
        {
            conversation = new DirectConversation { UserAId = userAId, UserBId = userBId };
            _db.DirectConversations.Add(conversation);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var other = await _db.Users.FirstAsync(u => u.Id == request.OtherUserId, cancellationToken);
        var presence = await _db.UserPresences.FirstOrDefaultAsync(p => p.UserId == request.OtherUserId, cancellationToken);

        return new DirectConversationDto(conversation.Id, other.Id, other.FirstName + " " + other.LastName,
            presence?.IsOnline ?? false, null, null, 0);
    }

    public static (Guid UserAId, Guid UserBId) NormalizePair(Guid userId1, Guid userId2) =>
        string.CompareOrdinal(userId1.ToString(), userId2.ToString()) <= 0 ? (userId1, userId2) : (userId2, userId1);
}

public record PostConversationMessageCommand(Guid ConversationId, Guid AuthorUserId, string Body, Guid? ParentMessageId) : IRequest<ChatMessageDto>;

public class PostConversationMessageCommandValidator : AbstractValidator<PostConversationMessageCommand>
{
    public PostConversationMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.AuthorUserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

public class PostConversationMessageCommandHandler : IRequestHandler<PostConversationMessageCommand, ChatMessageDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IChatRealtimeNotifier _realtime;

    public PostConversationMessageCommandHandler(IProjectFlowDbContext db, IMapper mapper, IChatRealtimeNotifier realtime)
    {
        _db = db; _mapper = mapper; _realtime = realtime;
    }

    public async Task<ChatMessageDto> Handle(PostConversationMessageCommand request, CancellationToken cancellationToken)
    {
        await ChatAccess.EnsureConversationParticipantAsync(_db, request.ConversationId, request.AuthorUserId, cancellationToken);

        if (request.ParentMessageId.HasValue && !await _db.ChatMessages.AnyAsync(m => m.Id == request.ParentMessageId.Value, cancellationToken))
            throw new NotFoundException("ChatMessage", request.ParentMessageId.Value);

        var message = new ChatMessage
        {
            DirectConversationId = request.ConversationId,
            AuthorUserId = request.AuthorUserId,
            Body = request.Body,
            ParentMessageId = request.ParentMessageId
        };
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await ChatAccess.MapMessageAsync(_db, _mapper, message.Id, cancellationToken);
        await _realtime.MessageReceivedAsync(null, request.ConversationId, dto, cancellationToken);
        return dto;
    }
}

// ---------------------------------------------------------------------------
// Reactions / attachments / delete — apply to either a channel or DM message
// ---------------------------------------------------------------------------

public record ToggleMessageReactionCommand(Guid MessageId, Guid UserId, string Emoji) : IRequest<Unit>;

public class ToggleMessageReactionCommandValidator : AbstractValidator<ToggleMessageReactionCommand>
{
    public ToggleMessageReactionCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Emoji).NotEmpty().MaximumLength(16);
    }
}

public class ToggleMessageReactionCommandHandler : IRequestHandler<ToggleMessageReactionCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IChatRealtimeNotifier _realtime;

    public ToggleMessageReactionCommandHandler(IProjectFlowDbContext db, IMapper mapper, IChatRealtimeNotifier realtime)
    {
        _db = db; _mapper = mapper; _realtime = realtime;
    }

    public async Task<Unit> Handle(ToggleMessageReactionCommand request, CancellationToken cancellationToken)
    {
        var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken)
            ?? throw new NotFoundException("ChatMessage", request.MessageId);

        if (message.ChannelId.HasValue)
            await ChatAccess.EnsureChannelMemberAsync(_db, message.ChannelId.Value, request.UserId, cancellationToken);
        else if (message.DirectConversationId.HasValue)
            await ChatAccess.EnsureConversationParticipantAsync(_db, message.DirectConversationId.Value, request.UserId, cancellationToken);

        // Toggle: reacting twice with the same emoji removes the reaction instead of erroring —
        // see ReactionToggleTests.
        var existing = await _db.MessageReactions.FirstOrDefaultAsync(
            r => r.MessageId == request.MessageId && r.UserId == request.UserId && r.Emoji == request.Emoji, cancellationToken);

        if (existing != null)
            _db.MessageReactions.Remove(existing);
        else
            _db.MessageReactions.Add(new MessageReaction { MessageId = request.MessageId, UserId = request.UserId, Emoji = request.Emoji });

        await _db.SaveChangesAsync(cancellationToken);

        var reactions = await _db.MessageReactions.Where(r => r.MessageId == request.MessageId)
            .GroupBy(r => r.Emoji)
            .Select(g => new MessageReactionGroupDto(g.Key, g.Select(x => x.UserId).ToList()))
            .ToListAsync(cancellationToken);

        await _realtime.ReactionChangedAsync(message.ChannelId, message.DirectConversationId, request.MessageId, reactions, cancellationToken);
        return Unit.Value;
    }
}

public record AddMessageAttachmentCommand(Guid MessageId, string FileName, string FileUrl, long FileSizeBytes, Guid UploadedByUserId) : IRequest<MessageAttachmentDto>;

public class AddMessageAttachmentCommandValidator : AbstractValidator<AddMessageAttachmentCommand>
{
    public AddMessageAttachmentCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FileUrl).NotEmpty();
        RuleFor(x => x.UploadedByUserId).NotEmpty();
    }
}

public class AddMessageAttachmentCommandHandler : IRequestHandler<AddMessageAttachmentCommand, MessageAttachmentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddMessageAttachmentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<MessageAttachmentDto> Handle(AddMessageAttachmentCommand request, CancellationToken cancellationToken)
    {
        var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken)
            ?? throw new NotFoundException("ChatMessage", request.MessageId);

        if (message.ChannelId.HasValue)
            await ChatAccess.EnsureChannelMemberAsync(_db, message.ChannelId.Value, request.UploadedByUserId, cancellationToken);
        else if (message.DirectConversationId.HasValue)
            await ChatAccess.EnsureConversationParticipantAsync(_db, message.DirectConversationId.Value, request.UploadedByUserId, cancellationToken);

        var attachment = new MessageAttachment
        {
            MessageId = request.MessageId,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileSizeBytes = request.FileSizeBytes,
            UploadedByUserId = request.UploadedByUserId
        };
        _db.MessageAttachments.Add(attachment);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MessageAttachmentDto>(attachment);
    }
}

public record DeleteMessageCommand(Guid MessageId, Guid ActorUserId) : IRequest<Unit>;

public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IChatRealtimeNotifier _realtime;

    public DeleteMessageCommandHandler(IProjectFlowDbContext db, IChatRealtimeNotifier realtime) { _db = db; _realtime = realtime; }

    public async Task<Unit> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken)
            ?? throw new NotFoundException("ChatMessage", request.MessageId);

        if (message.AuthorUserId != request.ActorUserId)
            throw new UnauthorizedDomainException("Only the author can delete this message.");

        message.IsDeleted = true;
        message.Body = string.Empty;
        await _db.SaveChangesAsync(cancellationToken);

        await _realtime.MessageDeletedAsync(message.ChannelId, message.DirectConversationId, message.Id, cancellationToken);
        return Unit.Value;
    }
}
