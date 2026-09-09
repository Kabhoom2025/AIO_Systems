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

public class LedgerAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LedgerAccountEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_LedgerAccounts_Returns_Seeded_Chart_With_Correct_Balances_From_Posted_Entry()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/ledger-accounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        Assert.NotNull(accounts);
        Assert.Equal(10, accounts!.Count);

        // 500,000 opening balance plus the seeded Bank Reconciliation module's 5,000
        // bank-interest entry, minus the seeded Payroll module's 246,600 net-pay credit
        // from PR-00001's Paid JournalEntry, plus the seeded POS module's net +180 (POS-00002
        // Completed nets +180; POS-00003 Completed-then-Refunded nets to 0).
        var cash = Assert.Single(accounts!, a => a.Code == "1000");
        Assert.Equal(258580m, cash.Balance);

        var equity = Assert.Single(accounts!, a => a.Code == "3000");
        Assert.Equal(-500000m, equity.Balance);

        // The seeded Draft JournalEntry (customer sale) must NOT affect balances yet, but the
        // seeded Sent CustomerInvoice (INV-00001, Accounts Receivable module) already has.
        var ar = Assert.Single(accounts!, a => a.Code == "1100");
        Assert.Equal(189000m, ar.Balance);

        // Payroll module's chart-of-accounts additions, reflecting PR-00001's Paid posting.
        var salaryExpense = Assert.Single(accounts!, a => a.Code == "5100");
        Assert.Equal(274000m, salaryExpense.Balance);

        var salariesPayable = Assert.Single(accounts!, a => a.Code == "2100");
        Assert.Equal(-27400m, salariesPayable.Balance);
    }

    [Fact]
    public async Task Post_LedgerAccount_With_Duplicate_Code_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.PostAsJsonAsync("/api/ledger-accounts", new CreateLedgerAccountDto
        {
            Code = "1000",
            Name = "Duplicate Cash",
            Type = "Asset"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Then_Put_Then_Delete_LedgerAccount_Full_Crud_Cycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var createResponse = await client.PostAsJsonAsync("/api/ledger-accounts", new CreateLedgerAccountDto
        {
            Code = "6000",
            Name = "Office Supplies Expense",
            Type = "Expense"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LedgerAccountDto>();

        var updateResponse = await client.PutAsJsonAsync($"/api/ledger-accounts/{created!.Id}", new UpdateLedgerAccountDto
        {
            Name = "Office Supplies Expense (Renamed)",
            Type = "Expense",
            IsActive = false
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<LedgerAccountDto>();
        Assert.Equal("Office Supplies Expense (Renamed)", updated!.Name);
        Assert.False(updated.IsActive);

        var deleteResponse = await client.DeleteAsync($"/api/ledger-accounts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
