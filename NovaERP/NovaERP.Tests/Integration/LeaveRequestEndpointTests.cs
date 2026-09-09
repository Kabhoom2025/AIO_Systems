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

public class LeaveRequestEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LeaveRequestEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_LeaveRequests_Returns_Seeded_Pending_And_Approved_Requests()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/leave-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var requests = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(requests);
        Assert.Equal(2, requests!.Count);
        Assert.Contains(requests!, r => r.Status == "Pending" && r.EmployeeName == "Priya Sharma");
        Assert.Contains(requests!, r => r.Status == "Approved" && r.EmployeeName == "Ramesh Iyer");
    }

    [Fact]
    public async Task Approve_Then_Reject_Then_Cancel_Only_Work_From_Pending()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var employeesResponse = await client.GetAsync("/api/employees");
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        var kavya = employees!.First(e => e.FirstName == "Kavya");

        var leaveTypesResponse = await client.GetAsync("/api/leave-types");
        var leaveTypes = await leaveTypesResponse.Content.ReadFromJsonAsync<List<LeaveTypeDto>>();
        var annual = leaveTypes!.First(t => t.Code == "ANNUAL");

        var createResponse = await client.PostAsJsonAsync("/api/leave-requests", new CreateLeaveRequestDto
        {
            EmployeeId = kavya.Id,
            LeaveTypeId = annual.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(20),
            EndDate = DateTime.UtcNow.Date.AddDays(21),
            DaysRequested = 2,
            Reason = "Personal"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.Equal("Pending", created!.Status);

        var approveResponse = await client.PostAsync($"/api/leave-requests/{created.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.Equal("Approved", approved!.Status);

        // Already Approved — Reject/Cancel/Edit/Delete should all now be rejected.
        var rejectResponse = await client.PostAsync($"/api/leave-requests/{created.Id}/reject", null);
        Assert.Equal(HttpStatusCode.BadRequest, rejectResponse.StatusCode);

        var cancelResponse = await client.PostAsync($"/api/leave-requests/{created.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/leave-requests/{created.Id}", new UpdateLeaveRequestDto
        {
            LeaveTypeId = annual.Id, StartDate = created.StartDate, EndDate = created.EndDate, DaysRequested = 3
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/leave-requests/{created.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_A_Pending_Request_Succeeds()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var requestsResponse = await client.GetAsync("/api/leave-requests");
        var requests = await requestsResponse.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        var pending = requests!.First(r => r.Status == "Pending");

        var cancelResponse = await client.PostAsync($"/api/leave-requests/{pending.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task Get_LeaveRequests_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/leave-requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
