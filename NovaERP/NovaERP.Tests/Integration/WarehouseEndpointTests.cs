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

public class WarehouseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WarehouseEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Warehouses_Returns_Seeded_Warehouses()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/warehouses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var warehouses = await response.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        Assert.NotNull(warehouses);
        Assert.Contains(warehouses!, w => w.Code == "WH-BLR");
        Assert.Contains(warehouses!, w => w.Code == "WH-PUN");
    }

    [Fact]
    public async Task Post_Warehouse_With_Duplicate_Code_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var existing = warehouses!.First();

        var response = await client.PostAsJsonAsync("/api/warehouses", new CreateWarehouseDto
        {
            BranchId = existing.BranchId,
            Name = "Duplicate Warehouse",
            Code = existing.Code
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_Warehouse_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var branchId = warehouses!.First().BranchId;

        var createResponse = await client.PostAsJsonAsync("/api/warehouses", new CreateWarehouseDto
        {
            BranchId = branchId,
            Name = "Chennai Overflow Warehouse",
            Code = "WH-CHN"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<WarehouseDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/warehouses/{created!.Id}", new UpdateWarehouseDto
        {
            BranchId = branchId,
            Name = "Chennai Overflow Warehouse (Renamed)",
            IsActive = false
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<WarehouseDto>();
        Assert.Equal("Chennai Overflow Warehouse (Renamed)", updated!.Name);
        Assert.False(updated.IsActive);

        var deleteResponse = await client.DeleteAsync($"/api/warehouses/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
