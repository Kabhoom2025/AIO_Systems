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

public class EmployeeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeeEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Employees_Returns_Seeded_Employees_With_Department_And_Manager_Links()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        Assert.NotNull(employees);
        // 5, not 4 — admin@novaerp.local is now also linked to an Employee record (EMP-00005),
        // added so the AI Assistant's create-Service-Ticket action works for admin too.
        Assert.Equal(5, employees!.Count);

        var priya = Assert.Single(employees!, e => e.FirstName == "Priya");
        Assert.Equal("Engineering", priya.DepartmentName);
        Assert.NotNull(priya.ReportingManagerId);
        Assert.Equal("Siva Bangaru", priya.ReportingManagerName);

        var siva = Assert.Single(employees!, e => e.FirstName == "Siva");
        Assert.NotNull(siva.UserId);
        Assert.Equal("Demo Manager", siva.UserName);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_Employee_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var departmentsResponse = await client.GetAsync("/api/departments");
        var departments = await departmentsResponse.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        var engineering = departments!.First(d => d.Code == "ENG");

        var createResponse = await client.PostAsJsonAsync("/api/employees", new CreateEmployeeDto
        {
            DepartmentId = engineering.Id,
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@novaerp.local",
            JobTitle = "QA Engineer",
            EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.StartsWith("EMP-", created!.EmployeeCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/employees/{created.Id}", new UpdateEmployeeDto
        {
            FirstName = "Test", LastName = "User (Updated)", Email = "test.user@novaerp.local",
            JobTitle = "Senior QA Engineer", EmploymentType = "Full-Time", DateOfJoining = created.DateOfJoining
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.Equal("Senior QA Engineer", updated!.JobTitle);

        var deleteResponse = await client.DeleteAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Terminate_Sets_Status_And_Blocks_Further_Edit_Or_Delete()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var departmentsResponse = await client.GetAsync("/api/departments");
        var departments = await departmentsResponse.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        var sales = departments!.First(d => d.Code == "SALES");

        var createResponse = await client.PostAsJsonAsync("/api/employees", new CreateEmployeeDto
        {
            DepartmentId = sales.Id,
            FirstName = "ToBe",
            LastName = "Terminated",
            Email = "tobe.terminated@novaerp.local",
            JobTitle = "Sales Rep",
            EmploymentType = "Full-Time",
            DateOfJoining = DateTime.UtcNow.Date
        });
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        var terminateResponse = await client.PostAsync($"/api/employees/{created!.Id}/terminate", null);
        Assert.Equal(HttpStatusCode.OK, terminateResponse.StatusCode);
        var terminated = await terminateResponse.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.Equal("Terminated", terminated!.Status);
        Assert.NotNull(terminated.TerminationDate);

        var updateResponse = await client.PutAsJsonAsync($"/api/employees/{created.Id}", new UpdateEmployeeDto
        {
            FirstName = "ToBe", LastName = "Terminated", Email = "tobe.terminated@novaerp.local",
            JobTitle = "Sales Rep", EmploymentType = "Full-Time", DateOfJoining = created.DateOfJoining
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Employees_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
