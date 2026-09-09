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

public class AssetEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AssetEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Assets_Returns_Seeded_Assets_With_Correct_Assignments()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/assets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assets = await response.Content.ReadFromJsonAsync<List<AssetDto>>();
        Assert.NotNull(assets);
        Assert.Equal(5, assets!.Count);

        Assert.Contains(assets!, a => a.AssetCode == "AST-00001" && a.AssignedToName == "Siva Bangaru" && a.Status == "Assigned");
        Assert.Contains(assets!, a => a.AssetCode == "AST-00003" && a.Status == "Available");
        Assert.Contains(assets!, a => a.AssetCode == "AST-00004" && a.Status == "UnderMaintenance");
        Assert.Contains(assets!, a => a.AssetCode == "AST-00005" && a.Status == "Retired");
    }

    [Fact]
    public async Task Assign_Then_Unassign_Full_Lifecycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var assetsResponse = await client.GetAsync("/api/assets");
        var assets = await assetsResponse.Content.ReadFromJsonAsync<List<AssetDto>>();
        var available = assets!.First(a => a.Status == "Available");

        var employeesResponse = await client.GetAsync("/api/employees");
        var employees = await employeesResponse.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        var employee = employees!.First();

        var assignResponse = await client.PostAsJsonAsync($"/api/assets/{available.Id}/assign", new AssignAssetDto { EmployeeId = employee.Id });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<AssetDto>();
        Assert.Equal("Assigned", assigned!.Status);
        Assert.Equal(employee.Id, assigned.AssignedToId);

        // Assigning again while already Assigned is rejected.
        var reassignResponse = await client.PostAsJsonAsync($"/api/assets/{available.Id}/assign", new AssignAssetDto { EmployeeId = employee.Id });
        Assert.Equal(HttpStatusCode.BadRequest, reassignResponse.StatusCode);

        var unassignResponse = await client.PostAsync($"/api/assets/{available.Id}/unassign", null);
        Assert.Equal(HttpStatusCode.OK, unassignResponse.StatusCode);
        var unassigned = await unassignResponse.Content.ReadFromJsonAsync<AssetDto>();
        Assert.Equal("Available", unassigned!.Status);
        Assert.Null(unassigned.AssignedToId);
    }

    [Fact]
    public async Task Retire_Is_Terminal_And_Blocks_Further_Edit_Or_Delete()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var categoriesResponse = await client.GetAsync("/api/asset-categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<AssetCategoryDto>>();
        var laptop = categories!.First(c => c.Code == "LAPTOP");

        var createResponse = await client.PostAsJsonAsync("/api/assets", new CreateAssetDto
        {
            Name = "ToBeRetired Laptop", CategoryId = laptop.Id, PurchaseDate = DateTime.UtcNow.Date
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AssetDto>();

        var retireResponse = await client.PostAsync($"/api/assets/{created!.Id}/retire", null);
        Assert.Equal(HttpStatusCode.OK, retireResponse.StatusCode);
        var retired = await retireResponse.Content.ReadFromJsonAsync<AssetDto>();
        Assert.Equal("Retired", retired!.Status);

        var updateResponse = await client.PutAsJsonAsync($"/api/assets/{created.Id}", new UpdateAssetDto
        {
            Name = "Renamed", CategoryId = laptop.Id, PurchaseDate = created.PurchaseDate
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/assets/{created.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        var secondRetireResponse = await client.PostAsync($"/api/assets/{created.Id}/retire", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondRetireResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Assets_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/assets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
