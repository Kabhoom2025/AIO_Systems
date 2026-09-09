using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;
using Xunit;

namespace NovaERP.Tests.Integration;

/// <summary>Exercises the AI Assistant's self-service scoping — a Conversation belongs to a
/// User, not a Role, so every action here is reachable with plain [Authorize] and no
/// permission policy; ownership is enforced in the service layer (403, not 404, for a
/// conversation that exists but isn't the caller's), the same shape as Dashboard Builder.
/// The test appsettings has no Anthropic:ApiKey configured, so every SendMessage call takes
/// the graceful "not configured" path — the only state testable without live credentials, and
/// also the real behavior for every user until a key is supplied.</summary>
public class AiAssistantEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AiAssistantEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

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
    public async Task Create_Then_SendMessage_Returns_Graceful_NotConfigured_Reply()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Test Chat" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var sendResponse = await client.PostAsJsonAsync($"/api/my-assistant/{created!.Id}/messages",
            new SendMessageDto { Content = "How many open service tickets do we have?" });
        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var conversation = await sendResponse.Content.ReadFromJsonAsync<ConversationDto>();

        Assert.Equal(2, conversation!.Messages.Count);
        Assert.Equal("user", conversation.Messages[0].Role);
        Assert.Equal("assistant", conversation.Messages[1].Role);
        Assert.Contains("isn't configured", conversation.Messages[1].Content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAll_Returns_Only_My_Own_Conversations()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Chat A" });

        var response = await client.GetAsync("/api/my-assistant");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var conversations = await response.Content.ReadFromJsonAsync<List<ConversationSummaryDto>>();
        Assert.Single(conversations!);
        Assert.Equal("Chat A", conversations![0].Title);
    }

    [Fact]
    public async Task Another_Users_Conversation_Is_403_Not_404()
    {
        await EnsureSeededAsync();

        var employeeClient = _factory.CreateClient();
        employeeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(employeeClient, "employee@novaerp.local", "Employee@123"));
        var createResponse = await employeeClient.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Private" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var managerClient = _factory.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(managerClient, "manager@novaerp.local", "Manager@123"));

        var response = await managerClient.GetAsync($"/api/my-assistant/{created!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Removes_The_Conversation()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "To Delete" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var deleteResponse = await client.DeleteAsync($"/api/my-assistant/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/my-assistant/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/my-assistant");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Seeds a Pending "CreateServiceTicket" action message directly via the DbContext,
    /// bypassing the need for a live/mocked LLM tool call — the propose step itself (parsing the
    /// model's tool arguments, resolving category/employee) isn't exercised here, only the
    /// confirm/cancel execution logic these tests target.</summary>
    private async Task<int> SeedPendingCreateTicketActionAsync(int conversationId, string categoryName, string requesterEmail)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();

        var category = await db.TicketCategories.FirstAsync(c => c.Name == categoryName);
        var employee = await db.Employees.Include(e => e.User).FirstAsync(e => e.User!.Email == requesterEmail);

        var payload = JsonSerializer.Serialize(new CreateServiceTicketDto
        {
            Subject = "Printer not working",
            Description = "The office printer on the 3rd floor is jammed.",
            CategoryId = category.Id,
            RequesterId = employee.Id,
            Priority = "Medium"
        });

        var message = new Message
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = "I'll create a Service Ticket. Confirm to proceed.",
            PendingActionType = "CreateServiceTicket",
            PendingActionPayloadJson = payload,
            PendingActionStatus = "Pending"
        };
        db.Set<Message>().Add(message);
        await db.SaveChangesAsync();
        return message.Id;
    }

    [Fact]
    public async Task Employee_Without_ServiceDeskCreate_Cannot_Confirm_A_Pending_Action()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Ticket Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var messageId = await SeedPendingCreateTicketActionAsync(created!.Id, "Hardware", "employee@novaerp.local");

        var confirmResponse = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{messageId}/confirm-action", null);

        Assert.Equal(HttpStatusCode.Forbidden, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task Manager_Confirming_Pending_Action_Creates_A_Real_Ticket()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "manager@novaerp.local", "Manager@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Ticket Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var beforeCount = (await (await client.GetAsync("/api/service-tickets")).Content.ReadFromJsonAsync<List<ServiceTicketDto>>())!.Count;

        var messageId = await SeedPendingCreateTicketActionAsync(created!.Id, "Hardware", "manager@novaerp.local");

        var confirmResponse = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{messageId}/confirm-action", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var conversation = await confirmResponse.Content.ReadFromJsonAsync<ConversationDto>();
        Assert.Contains("Confirmed", conversation!.Messages.First(m => m.Id == messageId).PendingActionStatus);
        Assert.Contains(conversation.Messages, m => m.Content.Contains("created Service Ticket", StringComparison.OrdinalIgnoreCase));

        var afterCount = (await (await client.GetAsync("/api/service-tickets")).Content.ReadFromJsonAsync<List<ServiceTicketDto>>())!.Count;
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact]
    public async Task Manager_Cancelling_Pending_Action_Does_Not_Create_A_Ticket()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "manager@novaerp.local", "Manager@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Ticket Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var beforeCount = (await (await client.GetAsync("/api/service-tickets")).Content.ReadFromJsonAsync<List<ServiceTicketDto>>())!.Count;

        var messageId = await SeedPendingCreateTicketActionAsync(created!.Id, "Hardware", "manager@novaerp.local");

        var cancelResponse = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{messageId}/cancel-action", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var conversation = await cancelResponse.Content.ReadFromJsonAsync<ConversationDto>();
        Assert.Equal("Cancelled", conversation!.Messages.First(m => m.Id == messageId).PendingActionStatus);

        var afterCount = (await (await client.GetAsync("/api/service-tickets")).Content.ReadFromJsonAsync<List<ServiceTicketDto>>())!.Count;
        Assert.Equal(beforeCount, afterCount);
    }

    [Fact]
    public async Task Confirming_An_Already_Resolved_Action_Fails()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "manager@novaerp.local", "Manager@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-assistant", new CreateConversationDto { Title = "Ticket Test" });
        var created = await createResponse.Content.ReadFromJsonAsync<ConversationDto>();

        var messageId = await SeedPendingCreateTicketActionAsync(created!.Id, "Hardware", "manager@novaerp.local");

        var firstConfirm = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{messageId}/confirm-action", null);
        Assert.Equal(HttpStatusCode.OK, firstConfirm.StatusCode);

        var secondConfirm = await client.PostAsync($"/api/my-assistant/{created.Id}/messages/{messageId}/confirm-action", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondConfirm.StatusCode);
    }
}
