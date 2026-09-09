using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectFlowAI.Application.Features.Chat;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Data;
using ProjectFlowAI.Tests.TestDoubles;
using ProjectFlowAI.Tests.Unit;
using Xunit;

namespace ProjectFlowAI.Tests.Integration;

/// <summary>End-to-end: post a channel message as one user -> verify a different member sees it
/// and the channel's unreadCount reflects it -> mark read -> verify unreadCount drops to 0.</summary>
public class ChatFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChatFlowTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, CapturingEmailSender>();
                services.RemoveAll<IChatRealtimeNotifier>();
                services.AddScoped<IChatRealtimeNotifier, NoOpChatRealtimeNotifier>();
            });
        });

    [Fact]
    public async Task PostMessage_UnreadCount_MarkRead_Flow_Works_End_To_End()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<ProjectFlowDbContext>();
        await db.Database.EnsureCreatedAsync();

        var org = new Organization { Name = $"Org-{Guid.NewGuid():N}", Slug = $"org-{Guid.NewGuid():N}" };
        db.Organizations.Add(org);
        var userA = new User { Email = $"a-{Guid.NewGuid():N}@example.com", PasswordHash = "x", FirstName = "A", LastName = "One", OrganizationId = org.Id };
        var userB = new User { Email = $"b-{Guid.NewGuid():N}@example.com", PasswordHash = "x", FirstName = "B", LastName = "Two", OrganizationId = org.Id };
        db.Users.AddRange(userA, userB);
        await db.SaveChangesAsync();

        var channel = await mediator.Send(new CreateChannelCommand(org.Id, null, "general", false, new[] { userB.Id }, userA.Id));
        Assert.Equal(2, channel.MemberCount);
        Assert.Equal(0, channel.UnreadCount);

        var posted = await mediator.Send(new PostChannelMessageCommand(channel.Id, userA.Id, "Hello team", null));
        Assert.Equal("Hello team", posted.Body);

        var channelsForB = await mediator.Send(new ListChatChannelsQuery(org.Id, null, userB.Id));
        var channelForB = Assert.Single(channelsForB);
        Assert.Equal(1, channelForB.UnreadCount);

        var messagesForB = await mediator.Send(new GetChannelMessagesQuery(channel.Id, userB.Id, null, 50));
        Assert.Contains(messagesForB.Messages, m => m.Id == posted.Id && m.AuthorUserId == userA.Id);

        await mediator.Send(new MarkChannelReadCommand(channel.Id, userB.Id));

        var channelsForBAfterRead = await mediator.Send(new ListChatChannelsQuery(org.Id, null, userB.Id));
        Assert.Equal(0, Assert.Single(channelsForBAfterRead).UnreadCount);
    }
}
