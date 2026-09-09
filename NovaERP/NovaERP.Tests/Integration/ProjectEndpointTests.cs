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

public class ProjectEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProjectEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client, string email = "admin@novaerp.local", string password = "Admin@123")
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task Get_Projects_Returns_Seeded_Projects_With_Correct_Task_Counts()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        Assert.NotNull(projects);
        Assert.Equal(3, projects!.Count);

        var erp = Assert.Single(projects!, p => p.Code == "PROJ-ERP");
        Assert.Equal("Siva Bangaru", erp.ManagerName);
        Assert.Equal(3, erp.TaskCount);
        Assert.Equal(1, erp.CompletedTaskCount);
    }

    [Fact]
    public async Task Employee_Role_Can_View_Projects_Since_Projects_Is_A_Foundation_Module()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_Project_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var employeesResponse = await client.GetAsync("/api/employees");
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        var manager = employees!.First();

        var createResponse = await client.PostAsJsonAsync("/api/projects", new CreateProjectDto
        {
            Code = "PROJ-TEST", Name = "Test Project", ManagerId = manager.Id,
            StartDate = DateTime.UtcNow.Date
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal("Planning", created!.Status);
        Assert.Equal(0, created.TaskCount);

        var updateResponse = await client.PutAsJsonAsync($"/api/projects/{created.Id}", new UpdateProjectDto
        {
            Name = "Test Project (Renamed)", ManagerId = manager.Id,
            StartDate = created.StartDate, Status = "Active"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal("Test Project (Renamed)", updated!.Name);
        Assert.Equal("Active", updated.Status);

        var deleteResponse = await client.DeleteAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Project_With_Tasks_Is_Rejected_Until_Tasks_Are_Removed()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var projectsResponse = await client.GetAsync("/api/projects");
        var projects = await projectsResponse.Content.ReadFromJsonAsync<List<ProjectDto>>();
        var erp = projects!.First(p => p.Code == "PROJ-ERP");

        var deleteResponse = await client.DeleteAsync($"/api/projects/{erp.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var tasksResponse = await client.GetAsync("/api/project-tasks");
        var tasks = await tasksResponse.Content.ReadFromJsonAsync<List<ProjectTaskDto>>();
        foreach (var task in tasks!.Where(t => t.ProjectId == erp.Id))
        {
            var taskDeleteResponse = await client.DeleteAsync($"/api/project-tasks/{task.Id}");
            Assert.Equal(HttpStatusCode.NoContent, taskDeleteResponse.StatusCode);
        }

        var secondDeleteResponse = await client.DeleteAsync($"/api/projects/{erp.Id}");
        Assert.Equal(HttpStatusCode.NoContent, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Projects_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
