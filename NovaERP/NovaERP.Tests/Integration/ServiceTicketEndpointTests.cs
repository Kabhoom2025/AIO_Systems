using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Infrastructure.Data;
using Xunit;

namespace NovaERP.Tests.Integration;

public class ServiceTicketEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ServiceTicketEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = "admin@novaerp.local", Password = "Admin@123" });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task Get_ServiceTickets_Returns_Seeded_Tickets_With_Correct_Statuses()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/service-tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tickets = await response.Content.ReadFromJsonAsync<List<ServiceTicketDto>>();
        Assert.NotNull(tickets);
        Assert.Equal(5, tickets!.Count);

        Assert.Contains(tickets!, t => t.TicketNumber == "TCK-00001" && t.Status == "Open" && t.AssignedToId == null);
        Assert.Contains(tickets!, t => t.TicketNumber == "TCK-00002" && t.Status == "InProgress" && t.AssignedToName == "Priya Sharma");
        Assert.Contains(tickets!, t => t.TicketNumber == "TCK-00003" && t.Status == "Resolved" && t.ResolutionNotes != null);
        Assert.Contains(tickets!, t => t.TicketNumber == "TCK-00004" && t.Status == "Closed");
        Assert.Contains(tickets!, t => t.TicketNumber == "TCK-00005" && t.Status == "Open" && t.Priority == "Critical");
    }

    [Fact]
    public async Task Assign_Moves_Open_Ticket_To_InProgress_And_Sets_AssignedTo()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ticketsResponse = await client.GetAsync("/api/service-tickets");
        var tickets = await ticketsResponse.Content.ReadFromJsonAsync<List<ServiceTicketDto>>();
        var openTicket = tickets!.First(t => t.TicketNumber == "TCK-00001");

        var employeesResponse = await client.GetAsync("/api/employees");
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        var employee = employees!.First();

        var assignResponse = await client.PostAsJsonAsync($"/api/service-tickets/{openTicket.Id}/assign", new AssignTicketDto { EmployeeId = employee.Id });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<ServiceTicketDto>();
        Assert.Equal("InProgress", assigned!.Status);
        Assert.Equal(employee.Id, assigned.AssignedToId);
    }

    [Fact]
    public async Task Resolve_Then_Close_Is_Terminal_And_Blocks_Update_Delete_And_Reopen()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ticketsResponse = await client.GetAsync("/api/service-tickets");
        var tickets = await ticketsResponse.Content.ReadFromJsonAsync<List<ServiceTicketDto>>();
        var inProgressTicket = tickets!.First(t => t.TicketNumber == "TCK-00002");

        var resolveResponse = await client.PostAsJsonAsync($"/api/service-tickets/{inProgressTicket.Id}/resolve",
            new ResolveTicketDto { ResolutionNotes = "Reinstalled VPN client and reset credentials." });
        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<ServiceTicketDto>();
        Assert.Equal("Resolved", resolved!.Status);
        Assert.NotNull(resolved.ResolvedDate);

        var closeResponse = await client.PostAsync($"/api/service-tickets/{inProgressTicket.Id}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<ServiceTicketDto>();
        Assert.Equal("Closed", closed!.Status);
        Assert.NotNull(closed.ClosedDate);

        var updateResponse = await client.PutAsJsonAsync($"/api/service-tickets/{inProgressTicket.Id}", new UpdateServiceTicketDto
        {
            Subject = "Renamed", Description = "Renamed", CategoryId = resolved.CategoryId, Priority = "Low"
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/service-tickets/{inProgressTicket.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var reopenResponse = await client.PostAsync($"/api/service-tickets/{inProgressTicket.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.BadRequest, reopenResponse.StatusCode);
    }

    [Fact]
    public async Task Reopen_From_Resolved_Returns_To_InProgress_When_Assigned()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ticketsResponse = await client.GetAsync("/api/service-tickets");
        var tickets = await ticketsResponse.Content.ReadFromJsonAsync<List<ServiceTicketDto>>();
        var resolvedTicket = tickets!.First(t => t.TicketNumber == "TCK-00003");
        Assert.NotNull(resolvedTicket.AssignedToId);

        var reopenResponse = await client.PostAsync($"/api/service-tickets/{resolvedTicket.Id}/reopen", null);
        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);
        var reopened = await reopenResponse.Content.ReadFromJsonAsync<ServiceTicketDto>();
        Assert.Equal("InProgress", reopened!.Status);
        Assert.Null(reopened.ResolvedDate);
    }

    [Fact]
    public async Task Get_ServiceTickets_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/service-tickets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
