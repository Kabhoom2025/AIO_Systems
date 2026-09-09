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

/// <summary>Exercises the Employee Portal's self-service scoping — every endpoint here is
/// reachable with plain [Authorize] (no hrms.view/payroll.view policy), and every query is
/// scoped to the caller's own linked Employee record via Employee.UserId, not a permission
/// check. Priya Sharma is seeded linked to employee@novaerp.local specifically to exercise
/// this live.</summary>
public class EmployeePortalEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeePortalEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task GetMyProfile_Returns_Own_Employee_Record()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.Equal("Priya", profile!.FirstName);
        Assert.Equal("Sharma", profile.LastName);
    }

    [Fact]
    public async Task GetMyProfile_Returns_404_When_No_Employee_Linked()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        // admin@novaerp.local is now linked to an Employee record (added so the AI Assistant's
        // create-Service-Ticket action works for admin too) — customer@novaerp.local remains
        // genuinely unlinked, so it's the right account for this "no linked Employee" case.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "customer@novaerp.local", "Customer@123"));

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaveRequests_Returns_Only_My_Own_Requests()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/my/leave-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var requests = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotEmpty(requests!);
        Assert.All(requests!, r => Assert.Equal("Priya Sharma", r.EmployeeName));
        Assert.DoesNotContain(requests!, r => r.EmployeeName == "Ramesh Iyer");
    }

    [Fact]
    public async Task GetMyPayslips_Returns_Only_My_Own_Paid_Lines()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/my/payslips");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payslips = await response.Content.ReadFromJsonAsync<List<MyPayslipDto>>();
        var payslip = Assert.Single(payslips!);
        Assert.Equal(75000m, payslip.GrossPay);
        Assert.Equal(67500m, payslip.NetPay);
    }

    [Fact]
    public async Task CreateMyLeaveRequest_Then_Cancel_Full_Self_Service_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var leaveTypesResponse = await client.GetAsync("/api/my/leave-types");
        var leaveTypes = await leaveTypesResponse.Content.ReadFromJsonAsync<List<LeaveTypeDto>>();
        var unpaid = leaveTypes!.First(t => t.Code == "UNPAID");

        var createResponse = await client.PostAsJsonAsync("/api/my/leave-requests", new CreateMyLeaveRequestDto
        {
            LeaveTypeId = unpaid.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(30),
            EndDate = DateTime.UtcNow.Date.AddDays(31),
            DaysRequested = 2,
            Reason = "Personal trip"
        });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.Equal("Priya Sharma", created!.EmployeeName);
        Assert.Equal("Pending", created.Status);

        var cancelResponse = await client.PostAsync($"/api/my/leave-requests/{created.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task Cancel_Another_Employees_LeaveRequest_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        // Ramesh's Approved request belongs to a different Employee than the caller (Priya).
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(adminClient, "admin@novaerp.local", "Admin@123"));
        var allRequestsResponse = await adminClient.GetAsync("/api/leave-requests");
        var allRequests = await allRequestsResponse.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        var rameshRequest = allRequests!.First(r => r.EmployeeName == "Ramesh Iyer");

        var cancelResponse = await client.PostAsync($"/api/my/leave-requests/{rameshRequest.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/my/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
