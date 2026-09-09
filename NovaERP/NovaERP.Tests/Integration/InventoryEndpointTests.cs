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

/// <summary>Boots the real API pipeline (auth, policies, controllers) against the isolated
/// EF Core InMemory database that Program.cs wires up for the "Testing" environment — no real
/// Postgres instance is required to exercise the new Product/StockMovement endpoints, including
/// the cross-module side effects PurchaseOrder.Receive/SalesOrder.Confirm now trigger.</summary>
public class InventoryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InventoryEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Products_Returns_Seeded_Products_With_Correct_OnHandQuantity()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.NotNull(products);

        var steelSheet = Assert.Single(products!, p => p.Sku == "STEEL-2MM");
        Assert.Equal(2000m, steelSheet.OnHandQuantity);

        var posTerminal = Assert.Single(products!, p => p.Sku == "POS-TERM");
        Assert.Equal(25m, posTerminal.OnHandQuantity);
    }

    [Fact]
    public async Task Post_StockMovement_Manual_Adjustment_Changes_OnHandQuantity()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var carton = products!.First(p => p.Sku == "CARTON-EXP");

        var adjustResponse = await client.PostAsJsonAsync("/api/stock-movements", new CreateStockMovementDto
        {
            ProductId = carton.Id,
            MovementType = "Adjustment",
            Quantity = -50m,
            Notes = "Stock take correction"
        });
        Assert.Equal(HttpStatusCode.OK, adjustResponse.StatusCode);

        var afterResponse = await client.GetAsync($"/api/products/{carton.Id}");
        var afterCarton = await afterResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(carton.OnHandQuantity - 50m, afterCarton!.OnHandQuantity);
    }

    [Fact]
    public async Task PurchaseOrder_Receive_Records_Receipt_Movement_And_Increases_OnHand()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var steelSheet = products!.First(p => p.Sku == "STEEL-2MM");
        var onHandBefore = steelSheet.OnHandQuantity;

        var ordersResponse = await client.GetAsync("/api/purchase-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
        var draftOrder = orders!.First(o => o.Status == "Draft" && o.Lines.Any(l => l.ProductId == steelSheet.Id));
        var lineQuantity = draftOrder.Lines.First(l => l.ProductId == steelSheet.Id).Quantity;

        await client.PostAsync($"/api/purchase-orders/{draftOrder.Id}/confirm", null);
        var receiveResponse = await client.PostAsync($"/api/purchase-orders/{draftOrder.Id}/receive", null);
        Assert.Equal(HttpStatusCode.OK, receiveResponse.StatusCode);

        var afterResponse = await client.GetAsync($"/api/products/{steelSheet.Id}");
        var afterProduct = await afterResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(onHandBefore + lineQuantity, afterProduct!.OnHandQuantity);

        var movementsResponse = await client.GetAsync($"/api/stock-movements?productId={steelSheet.Id}");
        var movements = await movementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        Assert.Contains(movements!, m => m.MovementType == "Receipt" && m.EntityType == "PurchaseOrder" && m.EntityId == draftOrder.Id);
    }

    [Fact]
    public async Task SalesOrder_Confirm_No_Longer_Touches_Stock()
    {
        // Stock deduction moved from SalesOrder.Confirm to Shipment.Ship once the Shipment
        // module introduced a real fulfillment stage — see ShipmentEndpointTests for the
        // Issue-movement assertion that used to live here.
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");
        var onHandBefore = posTerminal.OnHandQuantity;

        var accountsResponse = await client.GetAsync("/api/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountDto>>();
        var account = accounts!.First();

        var createResponse = await client.PostAsJsonAsync("/api/sales-orders", new CreateSalesOrderDto
        {
            AccountId = account.Id,
            OwnerId = account.OwnerId,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new() { ItemName = "POS Terminal Hardware", ProductId = posTerminal.Id, Quantity = 3m, UnitPrice = 18000m, DisplayOrder = 1 }
            }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<SalesOrderDto>();

        var confirmResponse = await client.PostAsync($"/api/sales-orders/{created!.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var afterResponse = await client.GetAsync($"/api/products/{posTerminal.Id}");
        var afterProduct = await afterResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(onHandBefore, afterProduct!.OnHandQuantity);

        var movementsResponse = await client.GetAsync($"/api/stock-movements?productId={posTerminal.Id}");
        var movements = await movementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        Assert.DoesNotContain(movements!, m => m.EntityType == "SalesOrder" && m.EntityId == created.Id);
    }

    [Fact]
    public async Task Get_Products_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
