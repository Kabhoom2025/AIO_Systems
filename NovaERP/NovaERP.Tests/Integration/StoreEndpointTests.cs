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

public class StoreEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StoreEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Stores_Returns_Seeded_Stores_With_Correct_Branch_And_Warehouse_Links()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/stores");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stores = await response.Content.ReadFromJsonAsync<List<StoreDto>>();
        Assert.NotNull(stores);
        Assert.Equal(2, stores!.Count);

        var blr = Assert.Single(stores!, s => s.Code == "STORE-BLR");
        Assert.Equal("Bengaluru Main Warehouse", blr.WarehouseName);
        Assert.NotEmpty(blr.BranchName);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_Store_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var storesResponse = await client.GetAsync("/api/stores");
        var stores = await storesResponse.Content.ReadFromJsonAsync<List<StoreDto>>();
        var existing = stores!.First();

        var createResponse = await client.PostAsJsonAsync("/api/stores", new CreateStoreDto
        {
            Name = "Downtown Store", Code = "STORE-DT", BranchId = existing.BranchId, WarehouseId = existing.WarehouseId
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<StoreDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/stores/{created!.Id}", new UpdateStoreDto
        {
            Name = "Downtown Store (Renamed)", BranchId = existing.BranchId, WarehouseId = existing.WarehouseId, IsActive = false
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<StoreDto>();
        Assert.Equal("Downtown Store (Renamed)", updated!.Name);
        Assert.False(updated.IsActive);

        var deleteResponse = await client.DeleteAsync($"/api/stores/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Stores_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/stores");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
