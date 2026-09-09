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

/// <summary>Exercises StockTransferService's dual-movement creation and its one genuinely new
/// business rule — a transfer can't exceed the source warehouse's on-hand quantity.</summary>
public class StockTransferEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StockTransferEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Post_StockTransfer_Creates_Issue_And_Receipt_Movements_With_Correct_Signs()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var cartons = products!.First(p => p.Sku == "CARTON-EXP");

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var fromWarehouse = warehouses!.First(w => w.Code == "WH-BLR");
        var toWarehouse = warehouses!.First(w => w.Code == "WH-PUN");

        var response = await client.PostAsJsonAsync("/api/stock-transfers", new CreateStockTransferDto
        {
            ProductId = cartons.Id,
            FromWarehouseId = fromWarehouse.Id,
            ToWarehouseId = toWarehouse.Id,
            Quantity = 100m,
            Notes = "Test transfer"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<StockTransferDto>();
        Assert.Equal(100m, created!.Quantity);

        var movementsResponse = await client.GetAsync($"/api/stock-movements?productId={cartons.Id}");
        var movements = await movementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();

        Assert.Contains(movements!, m =>
            m.MovementType == "Issue" && m.Quantity == -100m &&
            m.WarehouseId == fromWarehouse.Id && m.EntityType == "StockTransfer" && m.EntityId == created.Id);
        Assert.Contains(movements!, m =>
            m.MovementType == "Receipt" && m.Quantity == 100m &&
            m.WarehouseId == toWarehouse.Id && m.EntityType == "StockTransfer" && m.EntityId == created.Id);
    }

    [Fact]
    public async Task Post_StockTransfer_Exceeding_Source_OnHand_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM"); // seeded with no warehouse-specific stock at Pune

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var puneWarehouse = warehouses!.First(w => w.Code == "WH-PUN");
        var bengaluruWarehouse = warehouses!.First(w => w.Code == "WH-BLR");

        var response = await client.PostAsJsonAsync("/api/stock-transfers", new CreateStockTransferDto
        {
            ProductId = posTerminal.Id,
            FromWarehouseId = puneWarehouse.Id, // POS-TERM's opening stock is all at Bengaluru, none at Pune
            ToWarehouseId = bengaluruWarehouse.Id,
            Quantity = 1m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
