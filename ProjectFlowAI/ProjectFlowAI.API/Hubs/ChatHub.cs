using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.API.Hubs;

/// <summary>Realtime chat hub, mapped at /hubs/chat (see Program.cs — the ApiGateway forwards
/// /hubs/projectflow/{**catch-all} to /hubs/{**catch-all} on this service, so hub paths here must
/// stay exactly "/hubs/chat", not "/hubs/projectflow/chat"). Group naming convention:
/// "channel:{channelId}" for ChatChannel groups, "conversation:{conversationId}" for DM groups —
/// IChatRealtimeNotifier (see ChatHubNotifier) pushes to these same group names after a REST
/// command handler commits its DB write.</summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IProjectFlowDbContext _db;

    public ChatHub(IProjectFlowDbContext db) => _db = db;

    public static string ChannelGroup(Guid channelId) => $"channel:{channelId}";
    public static string ConversationGroup(Guid conversationId) => $"conversation:{conversationId}";
    private static string UserGroup(Guid userId) => $"chat-user:{userId}";

    private Guid? CurrentUserId
    {
        get
        {
            var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        if (userId.HasValue)
        {
            var channelIds = await _db.ChatChannelMembers.Where(m => m.UserId == userId.Value).Select(m => m.ChannelId).ToListAsync();
            foreach (var channelId in channelIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, ChannelGroup(channelId));

            var conversationIds = await _db.DirectConversations
                .Where(c => c.UserAId == userId.Value || c.UserBId == userId.Value)
                .Select(c => c.Id).ToListAsync();
            foreach (var conversationId in conversationIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));

            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId.Value));

            var presence = await _db.UserPresences.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (presence == null)
            {
                presence = new UserPresence { UserId = userId.Value };
                _db.UserPresences.Add(presence);
            }
            presence.IsOnline = true;
            presence.LastSeenAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            foreach (var channelId in channelIds)
                await Clients.OthersInGroup(ChannelGroup(channelId)).SendAsync("UserOnline", userId.Value);
            foreach (var conversationId in conversationIds)
                await Clients.OthersInGroup(ConversationGroup(conversationId)).SendAsync("UserOnline", userId.Value);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = CurrentUserId;
        if (userId.HasValue)
        {
            var presence = await _db.UserPresences.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (presence != null)
            {
                presence.IsOnline = false;
                presence.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            var channelIds = await _db.ChatChannelMembers.Where(m => m.UserId == userId.Value).Select(m => m.ChannelId).ToListAsync();
            foreach (var channelId in channelIds)
                await Clients.OthersInGroup(ChannelGroup(channelId)).SendAsync("UserOffline", userId.Value);

            var conversationIds = await _db.DirectConversations
                .Where(c => c.UserAId == userId.Value || c.UserBId == userId.Value)
                .Select(c => c.Id).ToListAsync();
            foreach (var conversationId in conversationIds)
                await Clients.OthersInGroup(ConversationGroup(conversationId)).SendAsync("UserOffline", userId.Value);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Purely ephemeral — no DB write. isChannel distinguishes a ChatChannel id from a DirectConversation id.</summary>
    public async Task Typing(Guid channelOrConversationId, bool isChannel)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue) return;

        var group = isChannel ? ChannelGroup(channelOrConversationId) : ConversationGroup(channelOrConversationId);
        // isChannel is echoed back so a client with both a channel and a DM open (or matching ids
        // across the two separate id spaces, however unlikely) can unambiguously route the event
        // instead of guessing from the id alone.
        await Clients.OthersInGroup(group).SendAsync("UserTyping", userId.Value, channelOrConversationId, isChannel);
    }
}
