using Microsoft.AspNetCore.SignalR;
using ProjectFlowAI.API.Hubs;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Realtime;

/// <summary>Implements IChatRealtimeNotifier (an Application-layer interface) using
/// IHubContext&lt;ChatHub&gt; — lives in the API project because SignalR Hub types can't be
/// referenced from Application/Infrastructure without introducing a circular project reference.</summary>
public class ChatHubNotifier : IChatRealtimeNotifier
{
    private readonly IHubContext<ChatHub> _hub;

    public ChatHubNotifier(IHubContext<ChatHub> hub) => _hub = hub;

    private static string GroupFor(Guid? channelId, Guid? directConversationId) =>
        channelId.HasValue ? ChatHub.ChannelGroup(channelId.Value) : ChatHub.ConversationGroup(directConversationId!.Value);

    public Task MessageReceivedAsync(Guid? channelId, Guid? directConversationId, object message, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(GroupFor(channelId, directConversationId)).SendAsync("MessageReceived", message, cancellationToken);

    public Task MessageDeletedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(GroupFor(channelId, directConversationId)).SendAsync("MessageDeleted", messageId, cancellationToken);

    public Task ReactionChangedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, object reactions, CancellationToken cancellationToken = default) =>
        _hub.Clients.Group(GroupFor(channelId, directConversationId)).SendAsync("ReactionChanged", messageId, reactions, cancellationToken);
}
