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

public class ProjectTaskEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProjectTaskEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_ProjectTasks_Returns_Seeded_Tasks_With_Correct_Assignments()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/project-tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<ProjectTaskDto>>();
        Assert.NotNull(tasks);
        Assert.Equal(6, tasks!.Count);

        Assert.Contains(tasks!, t => t.Title == "Design API contracts" && t.AssignedToName == "Priya Sharma");
        Assert.Contains(tasks!, t => t.Title == "Write onboarding docs" && t.AssignedToName == null);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_ProjectTask_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var projectsResponse = await client.GetAsync("/api/projects");
        var projects = await projectsResponse.Content.ReadFromJsonAsync<List<ProjectDto>>();
        var erp = projects!.First(p => p.Code == "PROJ-ERP");

        var createResponse = await client.PostAsJsonAsync("/api/project-tasks", new CreateProjectTaskDto
        {
            ProjectId = erp.Id, Title = "New task", Priority = "Medium", Status = "ToDo"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();
        Assert.Equal("ToDo", created!.Status);

        var employeesResponse = await client.GetAsync("/api/employees");
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        var assignee = employees!.First();

        var updateResponse = await client.PutAsJsonAsync($"/api/project-tasks/{created.Id}", new UpdateProjectTaskDto
        {
            AssignedToId = assignee.Id, Title = "New task (updated)", Priority = "High", Status = "InProgress"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectTaskDto>();
        Assert.Equal("InProgress", updated!.Status);
        Assert.Equal("High", updated.Priority);
        Assert.NotNull(updated.AssignedToName);

        // Confirm the parent project's TaskCount now reflects the new task.
        var projectResponse = await client.GetAsync($"/api/projects/{erp.Id}");
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal(4, project!.TaskCount);

        var deleteResponse = await client.DeleteAsync($"/api/project-tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Post_ProjectTask_With_Invalid_Status_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var projectsResponse = await client.GetAsync("/api/projects");
        var projects = await projectsResponse.Content.ReadFromJsonAsync<List<ProjectDto>>();
        var erp = projects!.First(p => p.Code == "PROJ-ERP");

        var response = await client.PostAsJsonAsync("/api/project-tasks", new CreateProjectTaskDto
        {
            ProjectId = erp.Id, Title = "Bad status task", Priority = "Medium", Status = "NotARealStatus"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ProjectTasks_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/project-tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
