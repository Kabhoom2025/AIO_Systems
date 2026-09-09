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

public class EmployeeCompensationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeeCompensationEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_EmployeeCompensation_Returns_Seeded_Rows_With_Correct_Gross_And_Net()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/employee-compensation");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rows = await response.Content.ReadFromJsonAsync<List<EmployeeCompensationDto>>();
        Assert.NotNull(rows);
        Assert.Equal(4, rows!.Count);

        var siva = Assert.Single(rows!, r => r.EmployeeName == "Siva Bangaru");
        Assert.Equal(120000m, siva.GrossPay);
        Assert.Equal(108000m, siva.NetPay);
    }

    [Fact]
    public async Task Post_Duplicate_Compensation_For_Same_Employee_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var rowsResponse = await client.GetAsync("/api/employee-compensation");
        var rows = await rowsResponse.Content.ReadFromJsonAsync<List<EmployeeCompensationDto>>();
        var existing = rows!.First();

        var response = await client.PostAsJsonAsync("/api/employee-compensation", new CreateEmployeeCompensationDto
        {
            EmployeeId = existing.EmployeeId, BasicSalary = 1000, Hra = 0, OtherAllowances = 0, Deductions = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_EmployeeCompensation_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var departmentsResponse = await client.GetAsync("/api/departments");
        var departments = await departmentsResponse.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        var engineering = departments!.First(d => d.Code == "ENG");

        // Every seeded Employee already has a compensation row, so a fresh Employee (with none
        // yet) is created here to exercise Create rather than colliding with the unique index.
        var newEmployeeResponse = await client.PostAsJsonAsync("/api/employees", new CreateEmployeeDto
        {
            DepartmentId = engineering.Id, FirstName = "Comp", LastName = "TestSubject",
            Email = "comp.testsubject@novaerp.local", JobTitle = "Test Engineer",
            EmploymentType = "Full-Time", DateOfJoining = DateTime.UtcNow.Date
        });
        var employeeWithoutComp = await newEmployeeResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        var createResponse = await client.PostAsJsonAsync("/api/employee-compensation", new CreateEmployeeCompensationDto
        {
            EmployeeId = employeeWithoutComp!.Id, BasicSalary = 40000, Hra = 16000, OtherAllowances = 4000, Deductions = 6000
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeCompensationDto>();
        Assert.Equal(60000m, created!.GrossPay);
        Assert.Equal(54000m, created.NetPay);

        var updateResponse = await client.PutAsJsonAsync($"/api/employee-compensation/{created.Id}", new UpdateEmployeeCompensationDto
        {
            BasicSalary = 45000, Hra = 18000, OtherAllowances = 4000, Deductions = 6000
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<EmployeeCompensationDto>();
        Assert.Equal(67000m, updated!.GrossPay);

        var deleteResponse = await client.DeleteAsync($"/api/employee-compensation/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_EmployeeCompensation_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/employee-compensation");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
