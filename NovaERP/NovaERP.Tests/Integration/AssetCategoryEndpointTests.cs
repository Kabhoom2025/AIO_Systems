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

public class AssetCategoryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AssetCategoryEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_AssetCategories_Returns_Seeded_Categories()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/asset-categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<AssetCategoryDto>>();
        Assert.NotNull(categories);
        Assert.Equal(3, categories!.Count);
        Assert.Contains(categories!, c => c.Code == "LAPTOP");
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_AssetCategory_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var createResponse = await client.PostAsJsonAsync("/api/asset-categories", new CreateAssetCategoryDto
        {
            Name = "Phone", Code = "PHONE"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<AssetCategoryDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/asset-categories/{created!.Id}", new UpdateAssetCategoryDto
        {
            Name = "Mobile Phone", IsActive = false
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<AssetCategoryDto>();
        Assert.Equal("Mobile Phone", updated!.Name);
        Assert.False(updated.IsActive);

        var deleteResponse = await client.DeleteAsync($"/api/asset-categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_AssetCategories_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/asset-categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
