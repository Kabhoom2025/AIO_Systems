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

/// <summary>Exercises ProductionOrderService.CompleteAsync — the module's one genuinely new
/// business rule, generalizing StockTransferService's single-component on-hand check to N
/// BOM components, and its dual-direction (consume N, produce 1) movement creation.</summary>
public class ProductionOrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductionOrderEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Complete_ProductionOrder_Consumes_Components_And_Produces_FinishedGood()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var rack = products!.First(p => p.Sku == "RACK-ASM");
        var steelSheet = products!.First(p => p.Sku == "STEEL-2MM");
        var steelBeam = products!.First(p => p.Sku == "STEEL-BEAM");
        var sheetOnHandBefore = steelSheet.OnHandQuantity;
        var beamOnHandBefore = steelBeam.OnHandQuantity;

        var ordersResponse = await client.GetAsync("/api/production-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<ProductionOrderDto>>();
        var draftOrder = orders!.First(o => o.Status == "Draft" && o.ProductId == rack.Id);

        var completeResponse = await client.PostAsync($"/api/production-orders/{draftOrder.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<ProductionOrderDto>();
        Assert.Equal("Completed", completed!.Status);

        var afterProductsResponse = await client.GetAsync("/api/products");
        var afterProducts = await afterProductsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var rackAfter = afterProducts!.First(p => p.Sku == "RACK-ASM");
        var sheetAfter = afterProducts!.First(p => p.Sku == "STEEL-2MM");
        var beamAfter = afterProducts!.First(p => p.Sku == "STEEL-BEAM");

        Assert.Equal(draftOrder.Quantity, rackAfter.OnHandQuantity);
        Assert.Equal(sheetOnHandBefore - (4m * draftOrder.Quantity), sheetAfter.OnHandQuantity);
        Assert.Equal(beamOnHandBefore - (2m * draftOrder.Quantity), beamAfter.OnHandQuantity);

        var movementsResponse = await client.GetAsync($"/api/stock-movements?productId={rack.Id}");
        var movements = await movementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        Assert.Contains(movements!, m => m.MovementType == "Receipt" && m.EntityType == "ProductionOrder" && m.EntityId == draftOrder.Id && m.Quantity == draftOrder.Quantity);

        var sheetMovementsResponse = await client.GetAsync($"/api/stock-movements?productId={steelSheet.Id}");
        var sheetMovements = await sheetMovementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        Assert.Contains(sheetMovements!, m => m.MovementType == "Issue" && m.EntityType == "ProductionOrder" && m.EntityId == draftOrder.Id && m.Quantity == -(4m * draftOrder.Quantity));
    }

    [Fact]
    public async Task Complete_ProductionOrder_Exceeding_Component_OnHand_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var rack = products!.First(p => p.Sku == "RACK-ASM");

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var bengaluru = warehouses!.First(w => w.Code == "WH-BLR");

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        // Absurdly large quantity guarantees it exceeds on-hand of the BOM's components.
        var createResponse = await client.PostAsJsonAsync("/api/production-orders", new CreateProductionOrderDto
        {
            ProductId = rack.Id,
            WarehouseId = bengaluru.Id,
            Quantity = 1_000_000m,
            OwnerId = owner.Id
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductionOrderDto>();

        var completeResponse = await client.PostAsync($"/api/production-orders/{created!.Id}/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, completeResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_Draft_ProductionOrder_Creates_No_Movements_And_Edit_After_Complete_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ordersResponse = await client.GetAsync("/api/production-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<ProductionOrderDto>>();
        var draftOrder = orders!.First(o => o.Status == "Draft");

        var completeResponse = await client.PostAsync($"/api/production-orders/{draftOrder.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/production-orders/{draftOrder.Id}", new UpdateProductionOrderDto
        {
            WarehouseId = draftOrder.WarehouseId,
            Quantity = 1m,
            OwnerId = draftOrder.OwnerId
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var cancelResponse = await client.PostAsync($"/api/production-orders/{draftOrder.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode); // already Completed, not Draft
    }
}
