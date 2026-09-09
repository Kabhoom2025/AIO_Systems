using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Features.Chat;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

/// <summary>No-op fake for IChatRealtimeNotifier — these tests only assert DB state, not the
/// SignalR broadcast (which is verified separately, see the hub verification notes in the report).</summary>
public class NoOpChatRealtimeNotifier : IChatRealtimeNotifier
{
    public Task MessageReceivedAsync(Guid? channelId, Guid? directConversationId, object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task MessageDeletedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ReactionChangedAsync(Guid? channelId, Guid? directConversationId, Guid messageId, object reactions, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class ChatCommandHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, User UserA, User UserB)> CreateUsersAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var userA = new User { Email = "a@example.com", PasswordHash = "x", FirstName = "A", LastName = "One", OrganizationId = org.Id };
        var userB = new User { Email = "b@example.com", PasswordHash = "x", FirstName = "B", LastName = "Two", OrganizationId = org.Id };
        db.Users.AddRange(userA, userB);
        await db.SaveChangesAsync();
        return (db, userA, userB);
    }

    // --- DM find-or-create normalization ---

    [Fact]
    public async Task CreateConversation_ABThenBA_Returns_The_Same_Conversation_Id()
    {
        var (db, userA, userB) = await CreateUsersAsync();
        var handler = new CreateConversationCommandHandler(db);

        var ab = await handler.Handle(new CreateConversationCommand(userA.Id, userB.Id), CancellationToken.None);
        var ba = await handler.Handle(new CreateConversationCommand(userB.Id, userA.Id), CancellationToken.None);

        Assert.Equal(ab.Id, ba.Id);
        Assert.Single(db.DirectConversations);
    }

    [Fact]
    public void NormalizePair_Always_Puts_The_Lexicographically_Smaller_Guid_First()
    {
        var g1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var g2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var (a1, b1) = CreateConversationCommandHandler.NormalizePair(g1, g2);
        var (a2, b2) = CreateConversationCommandHandler.NormalizePair(g2, g1);

        Assert.Equal(a1, a2);
        Assert.Equal(b1, b2);
    }

    // --- Reaction toggle ---

    [Fact]
    public async Task ToggleReaction_Twice_With_The_Same_Emoji_Removes_It()
    {
        var (db, userA, userB) = await CreateUsersAsync();
        var channel = new ChatChannel { OrganizationId = userA.OrganizationId!.Value, Name = "general", CreatedByUserId = userA.Id };
        db.ChatChannels.Add(channel);
        db.ChatChannelMembers.Add(new ChatChannelMember { ChannelId = channel.Id, UserId = userA.Id });
        db.ChatChannelMembers.Add(new ChatChannelMember { ChannelId = channel.Id, UserId = userB.Id });
        var message = new ChatMessage { ChannelId = channel.Id, AuthorUserId = userA.Id, Body = "hi" };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        var handler = new ToggleMessageReactionCommandHandler(db, CreateMapper(), new NoOpChatRealtimeNotifier());

        await handler.Handle(new ToggleMessageReactionCommand(message.Id, userB.Id, "👍"), CancellationToken.None);
        Assert.Single(db.MessageReactions);

        await handler.Handle(new ToggleMessageReactionCommand(message.Id, userB.Id, "👍"), CancellationToken.None);
        Assert.Empty(db.MessageReactions);
    }

    [Fact]
    public async Task ToggleReaction_With_Different_Emojis_From_The_Same_User_Keeps_Both()
    {
        var (db, userA, userB) = await CreateUsersAsync();
        var channel = new ChatChannel { OrganizationId = userA.OrganizationId!.Value, Name = "general", CreatedByUserId = userA.Id };
        db.ChatChannels.Add(channel);
        db.ChatChannelMembers.Add(new ChatChannelMember { ChannelId = channel.Id, UserId = userB.Id });
        var message = new ChatMessage { ChannelId = channel.Id, AuthorUserId = userA.Id, Body = "hi" };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        var handler = new ToggleMessageReactionCommandHandler(db, CreateMapper(), new NoOpChatRealtimeNotifier());

        await handler.Handle(new ToggleMessageReactionCommand(message.Id, userB.Id, "👍"), CancellationToken.None);
        await handler.Handle(new ToggleMessageReactionCommand(message.Id, userB.Id, "🎉"), CancellationToken.None);

        Assert.Equal(2, await db.MessageReactions.CountAsync());
    }
}
