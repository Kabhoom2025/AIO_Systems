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
/// Postgres instance is required to exercise the new Purchase Order endpoints.</summary>
public class PurchaseOrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseOrderEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_PurchaseOrders_Returns_Seeded_Draft_Confirmed_And_Received_Orders_With_Correct_Totals()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/purchase-orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
        Assert.NotNull(orders);

        var draft = Assert.Single(orders!, o => o.Status == "Draft");
        // 500 * 850 = 425000 subtotal, 18% tax = 76500
        Assert.Equal(425000m, draft.Subtotal);
        Assert.Equal(76500m, draft.TaxTotal);

        var received = Assert.Single(orders!, o => o.Status == "Received");
        Assert.Equal(625000m, received.Subtotal); // 50 * 12500
    }

    [Fact]
    public async Task Post_PurchaseOrder_Snapshots_TaxRate_From_TaxCode_And_Generates_PoNumber()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var vendorsResponse = await client.GetAsync("/api/vendors");
        var vendors = await vendorsResponse.Content.ReadFromJsonAsync<List<VendorDto>>();
        var vendor = vendors!.First();

        var taxCodesResponse = await client.GetAsync("/api/tax-codes");
        var taxCodes = await taxCodesResponse.Content.ReadFromJsonAsync<List<TaxCodeDto>>();
        var gst5 = taxCodes!.First(t => t.Code == "GST5");

        var response = await client.PostAsJsonAsync("/api/purchase-orders", new CreatePurchaseOrderDto
        {
            VendorId = vendor.Id,
            OwnerId = vendor.OwnerId,
            Lines = new List<CreatePurchaseOrderLineDto>
            {
                new() { ItemName = "Test Part", Quantity = 4m, UnitPrice = 50m, TaxCodeId = gst5.Id, DisplayOrder = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();
        Assert.NotNull(created);
        Assert.StartsWith("PO-", created!.PoNumber);
        Assert.Equal(5m, created.Lines[0].TaxRatePercent);
        Assert.Equal(200m, created.Lines[0].LineSubtotal);
        Assert.Equal(10m, created.Lines[0].LineTax);
    }

    [Fact]
    public async Task Confirm_Then_Receive_Full_Flow_Succeeds_And_Edit_After_Confirm_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ordersResponse = await client.GetAsync("/api/purchase-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
        var draft = orders!.First(o => o.Status == "Draft");

        var confirmResponse = await client.PostAsync($"/api/purchase-orders/{draft.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<PurchaseOrderDto>();
        Assert.Equal("Confirmed", confirmed!.Status);

        var updateResponse = await client.PutAsJsonAsync($"/api/purchase-orders/{draft.Id}", new UpdatePurchaseOrderDto
        {
            OwnerId = draft.OwnerId,
            Lines = new List<CreatePurchaseOrderLineDto> { new() { ItemName = "Should fail", Quantity = 1m, UnitPrice = 1m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var receiveResponse = await client.PostAsync($"/api/purchase-orders/{draft.Id}/receive", null);
        Assert.Equal(HttpStatusCode.OK, receiveResponse.StatusCode);
        var received = await receiveResponse.Content.ReadFromJsonAsync<PurchaseOrderDto>();
        Assert.Equal("Received", received!.Status);
    }

    [Fact]
    public async Task Cancel_Already_Received_PurchaseOrder_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ordersResponse = await client.GetAsync("/api/purchase-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
        var received = orders!.First(o => o.Status == "Received");

        var response = await client.PostAsync($"/api/purchase-orders/{received.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_PurchaseOrders_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/purchase-orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
