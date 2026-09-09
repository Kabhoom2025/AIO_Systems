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

/// <summary>Exercises CustomerInvoiceService's Send/ReceivePayment actions — the mirror image
/// of VendorBillService's Approve/Pay, with Debit/Credit reversed.</summary>
public class CustomerInvoiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CustomerInvoiceEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_CustomerInvoices_Returns_Seeded_Sent_Invoice_With_Correct_AR_Balance()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/customer-invoices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invoices = await response.Content.ReadFromJsonAsync<List<CustomerInvoiceDto>>();
        Assert.NotNull(invoices);

        var sent = Assert.Single(invoices!, i => i.InvoiceNumber == "INV-00001");
        Assert.Equal("Sent", sent.Status);
        Assert.Equal(189000m, sent.TotalAmount);
        Assert.NotNull(sent.PostedJournalEntryId);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var ar = accounts!.First(a => a.Code == "1100");
        Assert.Equal(189000m, ar.Balance);
    }

    [Fact]
    public async Task Send_CustomerInvoice_Posts_JournalEntry_And_Updates_Balances()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var invoicesResponse = await client.GetAsync("/api/customer-invoices");
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<List<CustomerInvoiceDto>>();
        var draft = invoices!.First(i => i.Status == "Draft");

        var sendResponse = await client.PostAsync($"/api/customer-invoices/{draft.Id}/send", null);
        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var sent = await sendResponse.Content.ReadFromJsonAsync<CustomerInvoiceDto>();
        Assert.Equal("Sent", sent!.Status);
        Assert.NotNull(sent.PostedJournalEntryId);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var ar = accounts!.First(a => a.Code == "1100");
        var revenue = accounts!.First(a => a.Code == "4000");
        Assert.Equal(189000m + 50000m, ar.Balance);
        // Revenue also carries the seeded POS module's net -180 credit (POS-00002 Completed;
        // POS-00003's Completed-then-Refunded pair nets to 0).
        Assert.Equal(-(189000m + 50000m + 180m), revenue.Balance);
    }

    [Fact]
    public async Task ReceivePayment_On_Sent_CustomerInvoice_Posts_Second_JournalEntry_And_Moves_Balance_To_Cash()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var invoicesResponse = await client.GetAsync("/api/customer-invoices");
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<List<CustomerInvoiceDto>>();
        var sentInvoice = invoices!.First(i => i.InvoiceNumber == "INV-00001");

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var cashBefore = cash.Balance;

        var payResponse = await client.PostAsJsonAsync($"/api/customer-invoices/{sentInvoice.Id}/receive-payment",
            new ReceiveCustomerInvoicePaymentDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
        var paid = await payResponse.Content.ReadFromJsonAsync<CustomerInvoiceDto>();
        Assert.Equal("Paid", paid!.Status);
        Assert.NotNull(paid.PaymentJournalEntryId);

        var afterAccountsResponse = await client.GetAsync("/api/ledger-accounts");
        var afterAccounts = await afterAccountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var arAfter = afterAccounts!.First(a => a.Code == "1100");
        var cashAfter = afterAccounts!.First(a => a.Code == "1000");

        Assert.Equal(0m, arAfter.Balance);
        Assert.Equal(cashBefore + 189000m, cashAfter.Balance);
    }

    [Fact]
    public async Task Edit_And_Cancel_After_Send_Are_Both_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var invoicesResponse = await client.GetAsync("/api/customer-invoices");
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<List<CustomerInvoiceDto>>();
        var sentInvoice = invoices!.First(i => i.InvoiceNumber == "INV-00001");

        var updateResponse = await client.PutAsJsonAsync($"/api/customer-invoices/{sentInvoice.Id}", new UpdateCustomerInvoiceDto
        {
            ReceivableLedgerAccountId = sentInvoice.ReceivableLedgerAccountId,
            OwnerId = sentInvoice.OwnerId,
            Lines = new List<CreateCustomerInvoiceLineDto> { new() { LedgerAccountId = sentInvoice.Lines[0].LedgerAccountId, Amount = 1m, DisplayOrder = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var cancelResponse = await client.PostAsync($"/api/customer-invoices/{sentInvoice.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task Get_CustomerInvoices_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/customer-invoices");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
