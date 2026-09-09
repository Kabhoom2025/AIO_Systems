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

public class BillOfMaterialEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BillOfMaterialEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_BillOfMaterials_Returns_Seeded_Rack_Bom_With_Two_Components()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/bill-of-materials");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var boms = await response.Content.ReadFromJsonAsync<List<BillOfMaterialDto>>();
        Assert.NotNull(boms);

        var rackBom = Assert.Single(boms!, b => b.ProductSku == "RACK-ASM");
        Assert.Equal(2, rackBom.Components.Count);
        Assert.Contains(rackBom.Components, c => c.ComponentProductSku == "STEEL-2MM" && c.Quantity == 4m);
        Assert.Contains(rackBom.Components, c => c.ComponentProductSku == "STEEL-BEAM" && c.Quantity == 2m);
    }

    [Fact]
    public async Task Post_BillOfMaterial_For_Product_That_Already_Has_One_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var rack = products!.First(p => p.Sku == "RACK-ASM");
        var carton = products!.First(p => p.Sku == "CARTON-EXP");

        var response = await client.PostAsJsonAsync("/api/bill-of-materials", new CreateBillOfMaterialDto
        {
            ProductId = rack.Id,
            Components = new List<CreateBomComponentDto>
            {
                new() { ComponentProductId = carton.Id, Quantity = 1m, DisplayOrder = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_BillOfMaterial_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");
        var carton = products!.First(p => p.Sku == "CARTON-EXP");

        var createResponse = await client.PostAsJsonAsync("/api/bill-of-materials", new CreateBillOfMaterialDto
        {
            ProductId = posTerminal.Id,
            Components = new List<CreateBomComponentDto>
            {
                new() { ComponentProductId = carton.Id, Quantity = 1m, DisplayOrder = 1 }
            }
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<BillOfMaterialDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/bill-of-materials/{created!.Id}", new UpdateBillOfMaterialDto
        {
            IsActive = false,
            Components = new List<CreateBomComponentDto>
            {
                new() { ComponentProductId = carton.Id, Quantity = 2m, DisplayOrder = 1 }
            }
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<BillOfMaterialDto>();
        Assert.False(updated!.IsActive);
        Assert.Equal(2m, updated.Components.Single().Quantity);

        var deleteResponse = await client.DeleteAsync($"/api/bill-of-materials/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
