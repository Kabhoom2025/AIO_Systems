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

/// <summary>Exercises VendorBillService's Approve/Pay actions — the module's one genuinely
/// new integration, directly mirroring ProductionOrderService.CompleteAsync's "build domain
/// objects and add them via the repository directly" pattern, here posting real JournalEntry
/// rows instead of StockMovement rows.</summary>
public class VendorBillEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VendorBillEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_VendorBills_Returns_Seeded_Approved_Bill_With_Correct_AP_Balance()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/vendor-bills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bills = await response.Content.ReadFromJsonAsync<List<VendorBillDto>>();
        Assert.NotNull(bills);

        var approved = Assert.Single(bills!, b => b.BillNumber == "BILL-00001");
        Assert.Equal("Approved", approved.Status);
        Assert.Equal(425000m, approved.TotalAmount);
        Assert.NotNull(approved.PostedJournalEntryId);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var ap = accounts!.First(a => a.Code == "2000");
        Assert.Equal(-425000m, ap.Balance);
    }

    [Fact]
    public async Task Approve_VendorBill_Posts_JournalEntry_And_Updates_Balances()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var billsResponse = await client.GetAsync("/api/vendor-bills");
        var bills = await billsResponse.Content.ReadFromJsonAsync<List<VendorBillDto>>();
        var draft = bills!.First(b => b.Status == "Draft");

        var approveResponse = await client.PostAsync($"/api/vendor-bills/{draft.Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        var approved = await approveResponse.Content.ReadFromJsonAsync<VendorBillDto>();
        Assert.Equal("Approved", approved!.Status);
        Assert.NotNull(approved.PostedJournalEntryId);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cogs = accounts!.First(a => a.Code == "5000");
        var ap = accounts!.First(a => a.Code == "2000");
        Assert.Equal(64100m, cogs.Balance);
        Assert.Equal(-425000m - 64100m, ap.Balance);
    }

    [Fact]
    public async Task Pay_Approved_VendorBill_Posts_Second_JournalEntry_And_Moves_Balance_To_Cash()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var billsResponse = await client.GetAsync("/api/vendor-bills");
        var bills = await billsResponse.Content.ReadFromJsonAsync<List<VendorBillDto>>();
        var approvedBill = bills!.First(b => b.BillNumber == "BILL-00001");

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var cashBefore = cash.Balance;

        var payResponse = await client.PostAsJsonAsync($"/api/vendor-bills/{approvedBill.Id}/pay",
            new PayVendorBillDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
        var paid = await payResponse.Content.ReadFromJsonAsync<VendorBillDto>();
        Assert.Equal("Paid", paid!.Status);
        Assert.NotNull(paid.PaymentJournalEntryId);

        var afterAccountsResponse = await client.GetAsync("/api/ledger-accounts");
        var afterAccounts = await afterAccountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var apAfter = afterAccounts!.First(a => a.Code == "2000");
        var cashAfter = afterAccounts!.First(a => a.Code == "1000");

        Assert.Equal(0m, apAfter.Balance);
        Assert.Equal(cashBefore - 425000m, cashAfter.Balance);
    }

    [Fact]
    public async Task Edit_And_Cancel_After_Approve_Are_Both_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var billsResponse = await client.GetAsync("/api/vendor-bills");
        var bills = await billsResponse.Content.ReadFromJsonAsync<List<VendorBillDto>>();
        var approvedBill = bills!.First(b => b.BillNumber == "BILL-00001");

        var updateResponse = await client.PutAsJsonAsync($"/api/vendor-bills/{approvedBill.Id}", new UpdateVendorBillDto
        {
            PayableLedgerAccountId = approvedBill.PayableLedgerAccountId,
            OwnerId = approvedBill.OwnerId,
            Lines = new List<CreateVendorBillLineDto> { new() { LedgerAccountId = approvedBill.Lines[0].LedgerAccountId, Amount = 1m, DisplayOrder = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var cancelResponse = await client.PostAsync($"/api/vendor-bills/{approvedBill.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task Get_VendorBills_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vendor-bills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
