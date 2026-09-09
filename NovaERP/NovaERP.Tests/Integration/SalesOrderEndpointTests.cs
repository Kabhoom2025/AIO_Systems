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
/// Postgres instance is required to exercise the new Sales Order endpoints.</summary>
public class SalesOrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SalesOrderEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_SalesOrders_Returns_Seeded_Draft_And_Confirmed_Orders_With_Correct_Totals()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/sales-orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<SalesOrderDto>>();
        Assert.NotNull(orders);

        var draft = Assert.Single(orders!, o => o.Status == "Draft");
        // 100 * 2500 + 3 * 15000 = 250000 + 45000 = 295000 subtotal, 18% tax = 53100
        Assert.Equal(295000m, draft.Subtotal);
        Assert.Equal(53100m, draft.TaxTotal);
        Assert.Equal(348100m, draft.GrandTotal);

        var confirmed = Assert.Single(orders!, o => o.Status == "Confirmed");
        Assert.Equal(180000m, confirmed.Subtotal);
    }

    [Fact]
    public async Task Post_SalesOrder_Snapshots_TaxRate_From_TaxCode_And_Generates_OrderNumber()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var accountsResponse = await client.GetAsync("/api/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountDto>>();
        var account = accounts!.First();

        var taxCodesResponse = await client.GetAsync("/api/tax-codes");
        var taxCodes = await taxCodesResponse.Content.ReadFromJsonAsync<List<TaxCodeDto>>();
        var gst18 = taxCodes!.First(t => t.Code == "GST18");

        var response = await client.PostAsJsonAsync("/api/sales-orders", new CreateSalesOrderDto
        {
            AccountId = account.Id,
            OwnerId = account.OwnerId,
            Lines = new List<CreateSalesOrderLineDto>
            {
                new() { ItemName = "Test Widget", Quantity = 2m, UnitPrice = 100m, TaxCodeId = gst18.Id, DisplayOrder = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SalesOrderDto>();
        Assert.NotNull(created);
        Assert.StartsWith("SO-", created!.OrderNumber);
        Assert.Equal(18m, created.Lines[0].TaxRatePercent);
        Assert.Equal(200m, created.Lines[0].LineSubtotal);
        Assert.Equal(36m, created.Lines[0].LineTax);
    }

    [Fact]
    public async Task Confirm_Then_Update_Draft_Order_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ordersResponse = await client.GetAsync("/api/sales-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<SalesOrderDto>>();
        var draft = orders!.First(o => o.Status == "Draft");

        var confirmResponse = await client.PostAsync($"/api/sales-orders/{draft.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<SalesOrderDto>();
        Assert.Equal("Confirmed", confirmed!.Status);

        var updateResponse = await client.PutAsJsonAsync($"/api/sales-orders/{draft.Id}", new UpdateSalesOrderDto
        {
            OwnerId = draft.OwnerId,
            Lines = new List<CreateSalesOrderLineDto> { new() { ItemName = "Should fail", Quantity = 1m, UnitPrice = 1m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var reconfirmResponse = await client.PostAsync($"/api/sales-orders/{draft.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.BadRequest, reconfirmResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_SalesOrder_Succeeds_And_Double_Cancel_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var ordersResponse = await client.GetAsync("/api/sales-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<SalesOrderDto>>();
        var order = orders!.First(o => o.Status == "Draft");

        var cancelResponse = await client.PostAsync($"/api/sales-orders/{order.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<SalesOrderDto>();
        Assert.Equal("Cancelled", cancelled!.Status);

        var secondCancelResponse = await client.PostAsync($"/api/sales-orders/{order.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondCancelResponse.StatusCode);
    }

    [Fact]
    public async Task Get_SalesOrders_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sales-orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
