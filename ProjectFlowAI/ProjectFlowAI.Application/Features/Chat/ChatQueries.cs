using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Application.Features.Chat;

public record MessageAttachmentFileResult(string FileName, string FileUrl);

public record GetMessageAttachmentFileQuery(Guid Id) : IRequest<MessageAttachmentFileResult>;

public class GetMessageAttachmentFileQueryHandler : IRequestHandler<GetMessageAttachmentFileQuery, MessageAttachmentFileResult>
{
    private readonly IProjectFlowDbContext _db;

    public GetMessageAttachmentFileQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<MessageAttachmentFileResult> Handle(GetMessageAttachmentFileQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _db.MessageAttachments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("MessageAttachment", request.Id);
        return new MessageAttachmentFileResult(attachment.FileName, attachment.FileUrl);
    }
}

public record ListChatChannelsQuery(Guid OrganizationId, Guid? ProjectId, Guid CurrentUserId) : IRequest<IReadOnlyList<ChatChannelDto>>;

public class ListChatChannelsQueryHandler : IRequestHandler<ListChatChannelsQuery, IReadOnlyList<ChatChannelDto>>
{
    private readonly IProjectFlowDbContext _db;

    public ListChatChannelsQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<ChatChannelDto>> Handle(ListChatChannelsQuery request, CancellationToken cancellationToken)
    {
        // Only channels the current user is a member of are visible — membership is the access
        // gate for chat, per the Phase 4 "chat.use" permission design.
        var myChannelIds = await _db.ChatChannelMembers
            .Where(m => m.UserId == request.CurrentUserId)
            .Select(m => m.ChannelId)
            .ToListAsync(cancellationToken);

        var query = _db.ChatChannels.Where(c => c.OrganizationId == request.OrganizationId && myChannelIds.Contains(c.Id));
        if (request.ProjectId.HasValue)
            query = query.Where(c => c.ProjectId == request.ProjectId.Value);

        var channels = await query.ToListAsync(cancellationToken);
        var channelIds = channels.Select(c => c.Id).ToList();

        var memberCounts = await _db.ChatChannelMembers.Where(m => channelIds.Contains(m.ChannelId))
            .GroupBy(m => m.ChannelId).Select(g => new { ChannelId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ChannelId, x => x.Count, cancellationToken);

        var readMarkers = await _db.ChannelReads.Where(r => channelIds.Contains(r.ChannelId) && r.UserId == request.CurrentUserId)
            .ToDictionaryAsync(r => r.ChannelId, r => r.LastReadAt, cancellationToken);

        var unreadCounts = new Dictionary<Guid, int>();
        foreach (var channelId in channelIds)
        {
            var since = readMarkers.TryGetValue(channelId, out var lastReadAt) ? lastReadAt : DateTime.MinValue;
            unreadCounts[channelId] = await _db.ChatMessages.CountAsync(
                m => m.ChannelId == channelId && !m.IsDeleted && m.CreatedAt > since, cancellationToken);
        }

        return channels.Select(c => new ChatChannelDto(c.Id, c.OrganizationId, c.ProjectId, c.Name, c.IsPrivate,
            memberCounts.GetValueOrDefault(c.Id), unreadCounts.GetValueOrDefault(c.Id), c.CreatedAt)).ToList();
    }
}

public record GetChannelMessagesQuery(Guid ChannelId, Guid CurrentUserId, Guid? Before, int Limit) : IRequest<ChatMessageListDto>;

public class GetChannelMessagesQueryHandler : IRequestHandler<GetChannelMessagesQuery, ChatMessageListDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetChannelMessagesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ChatMessageListDto> Handle(GetChannelMessagesQuery request, CancellationToken cancellationToken)
    {
        await ChatAccess.EnsureChannelMemberAsync(_db, request.ChannelId, request.CurrentUserId, cancellationToken);

        var query = _db.ChatMessages
            .Include(m => m.AuthorUser).Include(m => m.Reactions).Include(m => m.Attachments).Include(m => m.Replies)
            .Where(m => m.ChannelId == request.ChannelId);

        if (request.Before.HasValue)
        {
            var cursor = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.Before.Value, cancellationToken);
            if (cursor != null) query = query.Where(m => m.CreatedAt < cursor.CreatedAt);
        }

        var limit = request.Limit > 0 ? Math.Min(request.Limit, 200) : 50;
        var entities = await query.OrderByDescending(m => m.CreatedAt).Take(limit).ToListAsync(cancellationToken);

        return new ChatMessageListDto(entities.Select(e => _mapper.Map<ChatMessageDto>(e)).ToList());
    }
}

public record ListConversationsQuery(Guid OrganizationId, Guid CurrentUserId) : IRequest<IReadOnlyList<DirectConversationDto>>;

public class ListConversationsQueryHandler : IRequestHandler<ListConversationsQuery, IReadOnlyList<DirectConversationDto>>
{
    private readonly IProjectFlowDbContext _db;

    public ListConversationsQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<DirectConversationDto>> Handle(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        // Organization scoping: only conversations where the other participant belongs to this org.
        var orgUserIds = await _db.Users.Where(u => u.OrganizationId == request.OrganizationId).Select(u => u.Id).ToListAsync(cancellationToken);

        var conversations = await _db.DirectConversations
            .Where(c => (c.UserAId == request.CurrentUserId || c.UserBId == request.CurrentUserId)
                        && (orgUserIds.Contains(c.UserAId) && orgUserIds.Contains(c.UserBId)))
            .ToListAsync(cancellationToken);

        var result = new List<DirectConversationDto>();
        foreach (var conversation in conversations)
        {
            var otherUserId = conversation.UserAId == request.CurrentUserId ? conversation.UserBId : conversation.UserAId;
            var otherUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == otherUserId, cancellationToken);
            if (otherUser == null) continue;

            var presence = await _db.UserPresences.FirstOrDefaultAsync(p => p.UserId == otherUserId, cancellationToken);

            var lastMessage = await _db.ChatMessages.Where(m => m.DirectConversationId == conversation.Id)
                .OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync(cancellationToken);

            var readMarker = await _db.ConversationReads.FirstOrDefaultAsync(
                r => r.ConversationId == conversation.Id && r.UserId == request.CurrentUserId, cancellationToken);
            var since = readMarker?.LastReadAt ?? DateTime.MinValue;
            var unreadCount = await _db.ChatMessages.CountAsync(
                m => m.DirectConversationId == conversation.Id && !m.IsDeleted && m.CreatedAt > since, cancellationToken);

            result.Add(new DirectConversationDto(conversation.Id, otherUser.Id, otherUser.FirstName + " " + otherUser.LastName,
                presence?.IsOnline ?? false,
                lastMessage != null ? (lastMessage.IsDeleted ? null : Truncate(lastMessage.Body)) : null,
                lastMessage?.CreatedAt, unreadCount));
        }

        return result.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue).ToList();
    }

    private static string Truncate(string body) => body.Length <= 140 ? body : body[..140];
}

public record GetConversationMessagesQuery(Guid ConversationId, Guid CurrentUserId, Guid? Before, int Limit) : IRequest<ChatMessageListDto>;

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, ChatMessageListDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetConversationMessagesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ChatMessageListDto> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        await ChatAccess.EnsureConversationParticipantAsync(_db, request.ConversationId, request.CurrentUserId, cancellationToken);

        var query = _db.ChatMessages
            .Include(m => m.AuthorUser).Include(m => m.Reactions).Include(m => m.Attachments).Include(m => m.Replies)
            .Where(m => m.DirectConversationId == request.ConversationId);

        if (request.Before.HasValue)
        {
            var cursor = await _db.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.Before.Value, cancellationToken);
            if (cursor != null) query = query.Where(m => m.CreatedAt < cursor.CreatedAt);
        }

        var limit = request.Limit > 0 ? Math.Min(request.Limit, 200) : 50;
        var entities = await query.OrderByDescending(m => m.CreatedAt).Take(limit).ToListAsync(cancellationToken);

        // No dedicated "mark conversation read" endpoint exists in this phase's contract, so
        // viewing the conversation's most recent page of messages (Before == null) is what
        // advances the read marker that drives DirectConversationDto.UnreadCount.
        if (!request.Before.HasValue)
        {
            var readMarker = await _db.ConversationReads.FirstOrDefaultAsync(
                r => r.ConversationId == request.ConversationId && r.UserId == request.CurrentUserId, cancellationToken);
            if (readMarker == null)
            {
                readMarker = new Domain.Entities.ConversationRead { ConversationId = request.ConversationId, UserId = request.CurrentUserId };
                _db.ConversationReads.Add(readMarker);
            }
            readMarker.LastReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new ChatMessageListDto(entities.Select(e => _mapper.Map<ChatMessageDto>(e)).ToList());
    }
}
