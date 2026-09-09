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

/// <summary>Exercises PosSaleService's Complete/Refund/Cancel actions — the first module to
/// combine "decrement stock" and "post a payment" in a single completion step.</summary>
public class PosSaleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PosSaleEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_PosSales_Returns_Seeded_Sales_With_Correct_Statuses()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/pos-sales");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sales = await response.Content.ReadFromJsonAsync<List<PosSaleDto>>();
        Assert.NotNull(sales);
        Assert.Equal(3, sales!.Count);

        Assert.Contains(sales!, s => s.SaleNumber == "POS-00001" && s.Status == "Draft");
        var completed = Assert.Single(sales!, s => s.SaleNumber == "POS-00002");
        Assert.Equal("Completed", completed.Status);
        Assert.NotNull(completed.PostedJournalEntryId);
        Assert.Equal(180m, completed.TotalAmount);
        Assert.Contains(sales!, s => s.SaleNumber == "POS-00003" && s.Status == "Refunded");
    }

    [Fact]
    public async Task Complete_Draft_Sale_Decrements_Stock_And_Posts_Balanced_JournalEntry()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminal = products!.First(p => p.Sku == "POS-TERM");
        var onHandBefore = posTerminal.OnHandQuantity;

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var revenue = accounts!.First(a => a.Code == "4000");
        var cashBefore = cash.Balance;
        var revenueBefore = revenue.Balance;

        var salesResponse = await client.GetAsync("/api/pos-sales");
        var sales = await salesResponse.Content.ReadFromJsonAsync<List<PosSaleDto>>();
        var draft = sales!.First(s => s.SaleNumber == "POS-00001");

        var completeResponse = await client.PostAsJsonAsync($"/api/pos-sales/{draft.Id}/complete",
            new CompletePosSaleDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<PosSaleDto>();
        Assert.Equal("Completed", completed!.Status);
        Assert.NotNull(completed.PostedJournalEntryId);

        var total = draft.TotalAmount;

        var afterProductsResponse = await client.GetAsync("/api/products");
        var afterProducts = await afterProductsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var posTerminalAfter = afterProducts!.First(p => p.Sku == "POS-TERM");
        Assert.Equal(onHandBefore - draft.Lines[0].Quantity, posTerminalAfter.OnHandQuantity);

        var afterAccountsResponse = await client.GetAsync("/api/ledger-accounts");
        var afterAccounts = await afterAccountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cashAfter = afterAccounts!.First(a => a.Code == "1000");
        var revenueAfter = afterAccounts!.First(a => a.Code == "4000");
        Assert.Equal(cashBefore + total, cashAfter.Balance);
        Assert.Equal(revenueBefore - total, revenueAfter.Balance);
    }

    [Fact]
    public async Task Complete_Rejects_When_Insufficient_Stock()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var steelBeam = products!.First(p => p.Sku == "STEEL-BEAM");

        var warehousesResponse = await client.GetAsync("/api/warehouses");
        var warehouses = await warehousesResponse.Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var warehouse = warehouses!.First(w => w.Code == "WH-BLR");

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var revenue = accounts!.First(a => a.Code == "4000");
        var cash = accounts!.First(a => a.Code == "1000");

        var createResponse = await client.PostAsJsonAsync("/api/pos-sales", new CreatePosSaleDto
        {
            WarehouseId = warehouse.Id, RevenueLedgerAccountId = revenue.Id, OwnerId = 1,
            Lines = new List<CreatePosSaleLineDto> { new() { ProductId = steelBeam.Id, Quantity = 100000m, UnitPrice = 12500m } }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<PosSaleDto>();

        var completeResponse = await client.PostAsJsonAsync($"/api/pos-sales/{created!.Id}/complete",
            new CompletePosSaleDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.BadRequest, completeResponse.StatusCode);
    }

    [Fact]
    public async Task Refund_Completed_Sale_Restocks_And_Posts_Reversing_JournalEntry()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var productsResponse = await client.GetAsync("/api/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var carton = products!.First(p => p.Sku == "CARTON-EXP");
        var onHandBefore = carton.OnHandQuantity;

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var revenue = accounts!.First(a => a.Code == "4000");
        var cashBefore = cash.Balance;
        var revenueBefore = revenue.Balance;

        var salesResponse = await client.GetAsync("/api/pos-sales");
        var sales = await salesResponse.Content.ReadFromJsonAsync<List<PosSaleDto>>();
        var completed = sales!.First(s => s.SaleNumber == "POS-00002");
        var total = completed.TotalAmount;

        var refundResponse = await client.PostAsync($"/api/pos-sales/{completed.Id}/refund", null);
        Assert.Equal(HttpStatusCode.OK, refundResponse.StatusCode);
        var refunded = await refundResponse.Content.ReadFromJsonAsync<PosSaleDto>();
        Assert.Equal("Refunded", refunded!.Status);

        var afterProductsResponse = await client.GetAsync("/api/products");
        var afterProducts = await afterProductsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        var cartonAfter = afterProducts!.First(p => p.Sku == "CARTON-EXP");
        Assert.Equal(onHandBefore + completed.Lines[0].Quantity, cartonAfter.OnHandQuantity);

        var afterAccountsResponse = await client.GetAsync("/api/ledger-accounts");
        var afterAccounts = await afterAccountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cashAfter = afterAccounts!.First(a => a.Code == "1000");
        var revenueAfter = afterAccounts!.First(a => a.Code == "4000");
        Assert.Equal(cashBefore - total, cashAfter.Balance);
        Assert.Equal(revenueBefore + total, revenueAfter.Balance);
    }

    [Fact]
    public async Task Cancel_Only_Works_From_Draft_And_Locks_Update_Delete_Once_Completed()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var salesResponse = await client.GetAsync("/api/pos-sales");
        var sales = await salesResponse.Content.ReadFromJsonAsync<List<PosSaleDto>>();
        var draft = sales!.First(s => s.SaleNumber == "POS-00001");

        var cancelResponse = await client.PostAsync($"/api/pos-sales/{draft.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<PosSaleDto>();
        Assert.Equal("Cancelled", cancelled!.Status);

        var secondCancelResponse = await client.PostAsync($"/api/pos-sales/{draft.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondCancelResponse.StatusCode);

        var completed = sales!.First(s => s.SaleNumber == "POS-00002");

        var updateResponse = await client.PutAsJsonAsync($"/api/pos-sales/{completed.Id}", new UpdatePosSaleDto
        {
            WarehouseId = completed.WarehouseId, RevenueLedgerAccountId = completed.RevenueLedgerAccountId, OwnerId = completed.OwnerId,
            Lines = new List<CreatePosSaleLineDto> { new() { ProductId = completed.Lines[0].ProductId, Quantity = 1m, UnitPrice = 1m } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/pos-sales/{completed.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Get_PosSales_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/pos-sales");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
