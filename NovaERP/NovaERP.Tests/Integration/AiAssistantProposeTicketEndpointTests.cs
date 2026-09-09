using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;
using NovaERP.Tests.TestDoubles;
using Xunit;

namespace NovaERP.Tests.Integration;

/// <summary>Exercises AiAssistantService's real propose_create_service_ticket code path end to
/// end through SendMessage — using a fake IAiProvider (FakeProposeTicketAiProvider) instead of a
/// live LLM, since that's the only way to reach this branch without real credentials. This is
/// deliberately separate from AiAssistantEndpointTests' confirm/cancel tests, which insert a
/// Pending message directly and never exercise the propose step itself — that gap is exactly
/// what let a real bug (the proposed Message never had ConversationId set, so saving it 500'd
/// with a Messages FK violation) ship untested.</summary>
public class AiAssistantProposeTicketEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AiAssistantProposeTicketEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiProvider>();
                services.AddScoped<IAiProvider, FakeProposeTicketAiProvider>();
            });
        });

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task Manager_Asking_To_Create_A_Ticket_Gets_A_Real_Pending_Proposal()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "manager@novaerp.local", "Manager@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Propose Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var sendResponse = await client.PostAsJsonAsync($"/api/my-assistant/{created!.Id}/messages",
            new SendMessageDto { Content = "Please log a ticket, the office printer is jammed." });

        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var conversation = await sendResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var proposal = conversation!.Messages.Single(m => m.Role == "assistant");
        Assert.Equal("Pending", proposal.PendingActionStatus);
        Assert.Contains("Printer not working", proposal.Content);

        // Confirming it should succeed exactly like the DB-seeded confirm tests — proving the
        // propose step really did produce a valid, confirmable Message (ConversationId set,
        // valid CategoryId/RequesterId resolved).
        var confirmResponse = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{proposal.Id}/confirm-action", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task Employee_Without_Permission_Never_Gets_A_Ticket_Proposal()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Propose Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var sendResponse = await client.PostAsJsonAsync($"/api/my-assistant/{created!.Id}/messages",
            new SendMessageDto { Content = "Please log a ticket, the office printer is jammed." });

        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var conversation = await sendResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var reply = conversation!.Messages.Single(m => m.Role == "assistant");
        Assert.Null(reply.PendingActionStatus);
    }
}
