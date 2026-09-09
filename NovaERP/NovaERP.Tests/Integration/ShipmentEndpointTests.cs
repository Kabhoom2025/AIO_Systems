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

/// <summary>Exercises the Shipment status lifecycle (Open -> Picked -> Shipped -> Delivered,
/// or Cancelled from Open/Picked) and ShipmentService.ShipAsync — the module's core business
/// logic and the new home for the stock deduction that used to happen at SalesOrder.Confirm.
/// The Open/Picked/Shipped/Delivered/Cancelled vocabulary is deliberately TMS-friendly: an
/// external TMS integration can poll or push against these same status values.</summary>
public class ShipmentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ShipmentEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Ship_SalesOrder_Sourced_Shipment_Records_IssueOnly_Movement_And_Decreases_OnHand()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");
        var onHandBefore = posTerminal.OnHandQuantity;

        var shipmentsResponse = await client.GetAsync("/api/shipments");
        var shipments = await shipmentsResponse.Content.ReadFromJsonAsync<List<ShipmentDto>>();
        var open = shipments!.First(s => s.SourceType == "SalesOrder" && s.Status == "Open");

        var pickResponse = await client.PostAsync($"/api/shipments/{open.Id}/pick", null);
        Assert.Equal(HttpStatusCode.OK, pickResponse.StatusCode);
        var picked = await pickResponse.Content.ReadFromJsonAsync<ShipmentDto>();
        Assert.Equal("Picked", picked!.Status);

        var shipResponse = await client.PostAsync($"/api/shipments/{open.Id}/ship", null);
        Assert.Equal(HttpStatusCode.OK, shipResponse.StatusCode);
        var shipped = await shipResponse.Content.ReadFromJsonAsync<ShipmentDto>();
        Assert.Equal("Shipped", shipped!.Status);

        var afterResponse = await client.GetAsync($"/api/products/{posTerminal.Id}");
        var afterProduct = await afterResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(onHandBefore - 10m, afterProduct!.OnHandQuantity);

        var movementsResponse = await client.GetAsync($"/api/stock-movements?productId={posTerminal.Id}");
        var movements = await movementsResponse.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        Assert.Contains(movements!, m => m.MovementType == "Issue" && m.EntityType == "Shipment" && m.EntityId == open.Id && m.Quantity == -10m);
        Assert.DoesNotContain(movements!, m => m.MovementType == "Receipt" && m.EntityType == "Shipment" && m.EntityId == open.Id);
    }

    [Fact]
    public async Task Ship_Without_Picking_First_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var shipmentsResponse = await client.GetAsync("/api/shipments");
        var shipments = await shipmentsResponse.Content.ReadFromJsonAsync<List<ShipmentDto>>();
        var open = shipments!.First(s => s.SourceType == "SalesOrder" && s.Status == "Open");

        var shipResponse = await client.PostAsync($"/api/shipments/{open.Id}/ship", null);

        Assert.Equal(HttpStatusCode.BadRequest, shipResponse.StatusCode);
    }

    [Fact]
    public async Task Full_Lifecycle_Deliver_Requires_Shipped_And_Cancel_Not_Allowed_After_Ship()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var shipmentsResponse = await client.GetAsync("/api/shipments");
        var shipments = await shipmentsResponse.Content.ReadFromJsonAsync<List<ShipmentDto>>();
        var open = shipments!.First(s => s.SourceType == "SalesOrder" && s.Status == "Open");

        var deliverBeforeShipResponse = await client.PostAsync($"/api/shipments/{open.Id}/deliver", null);
        Assert.Equal(HttpStatusCode.BadRequest, deliverBeforeShipResponse.StatusCode);

        await client.PostAsync($"/api/shipments/{open.Id}/pick", null);
        var shipResponse = await client.PostAsync($"/api/shipments/{open.Id}/ship", null);
        Assert.Equal(HttpStatusCode.OK, shipResponse.StatusCode);

        var cancelAfterShipResponse = await client.PostAsync($"/api/shipments/{open.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelAfterShipResponse.StatusCode);

        var deliverResponse = await client.PostAsync($"/api/shipments/{open.Id}/deliver", null);
        Assert.Equal(HttpStatusCode.OK, deliverResponse.StatusCode);
        var delivered = await deliverResponse.Content.ReadFromJsonAsync<ShipmentDto>();
        Assert.Equal("Delivered", delivered!.Status);
    }

    [Fact]
    public async Task Cancel_Works_From_Open_And_From_Picked()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var bengaluru = warehouses!.First(w => w.Code == "WH-BLR");
        var pune = warehouses!.First(w => w.Code == "WH-PUN");

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        async Task<ShipmentDto> CreateTransferShipmentAsync()
        {
            var createResponse = await client.PostAsJsonAsync("/api/shipments", new CreateShipmentDto
            {
                WarehouseId = bengaluru.Id,
                SourceType = "TransferOrder",
                DestinationWarehouseId = pune.Id,
                OwnerId = owner.Id,
                Lines = new List<CreateShipmentLineDto> { new() { ProductId = posTerminal.Id, Quantity = 1m, DisplayOrder = 1 } }
            });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            return (await createResponse.Content.ReadFromJsonAsync<ShipmentDto>())!;
        }

        var openShipment = await CreateTransferShipmentAsync();
        var cancelOpenResponse = await client.PostAsync($"/api/shipments/{openShipment.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelOpenResponse.StatusCode);

        var pickedShipment = await CreateTransferShipmentAsync();
        await client.PostAsync($"/api/shipments/{pickedShipment.Id}/pick", null);
        var cancelPickedResponse = await client.PostAsync($"/api/shipments/{pickedShipment.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelPickedResponse.StatusCode);
        var cancelled = await cancelPickedResponse.Content.ReadFromJsonAsync<ShipmentDto>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task Ship_Exceeding_OnHand_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var bengaluru = warehouses!.First(w => w.Code == "WH-BLR");
        var pune = warehouses!.First(w => w.Code == "WH-PUN");

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        var createResponse = await client.PostAsJsonAsync("/api/shipments", new CreateShipmentDto
        {
            WarehouseId = bengaluru.Id,
            SourceType = "TransferOrder",
            DestinationWarehouseId = pune.Id,
            OwnerId = owner.Id,
            Lines = new List<CreateShipmentLineDto> { new() { ProductId = posTerminal.Id, Quantity = 1_000_000m, DisplayOrder = 1 } }
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipmentDto>();

        await client.PostAsync($"/api/shipments/{created!.Id}/pick", null);
        var shipResponse = await client.PostAsync($"/api/shipments/{created.Id}/ship", null);

        Assert.Equal(HttpStatusCode.BadRequest, shipResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Shipments_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/shipments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
