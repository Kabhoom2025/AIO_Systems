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

/// <summary>Exercises JournalEntryService's Post/Void actions and the module's one genuinely
/// new invariant — a journal entry's debits must equal its credits, enforced at save time by
/// the validator (not a DB-backed check, since it's a pure sum of the submitted lines).</summary>
public class JournalEntryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public JournalEntryEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Post_JournalEntry_Transitions_Draft_To_Posted_And_Updates_Account_Balances()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var entriesResponse = await client.GetAsync("/api/journal-entries");
        var entries = await entriesResponse.Content.ReadFromJsonAsync<List<JournalEntryDto>>();
        var draft = entries!.First(e => e.Status == "Draft");

        var postResponse = await client.PostAsync($"/api/journal-entries/{draft.Id}/post", null);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        var posted = await postResponse.Content.ReadFromJsonAsync<JournalEntryDto>();
        Assert.Equal("Posted", posted!.Status);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var ar = accounts!.First(a => a.Code == "1100");
        var revenue = accounts!.First(a => a.Code == "4000");
        // Plus the seeded Sent CustomerInvoice (INV-00001, Accounts Receivable module), which
        // already contributes 189,000 to both accounts before this entry is posted.
        Assert.Equal(189000m + 100000m, ar.Balance);
        // Revenue also carries the seeded POS module's net -180 credit (see PosSaleEndpointTests).
        Assert.Equal(-(189000m + 100000m + 180m), revenue.Balance);
    }

    [Fact]
    public async Task Post_JournalEntry_With_Unbalanced_Lines_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var equity = accounts!.First(a => a.Code == "3000");

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        var response = await client.PostAsJsonAsync("/api/journal-entries", new CreateJournalEntryDto
        {
            OwnerId = owner.Id,
            Description = "Deliberately unbalanced",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { LedgerAccountId = cash.Id, Debit = 100m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = equity.Id, Debit = 0m, Credit = 50m, DisplayOrder = 2 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Edit_After_Post_Is_Rejected_And_Void_Excludes_Lines_From_Balances()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var entriesResponse = await client.GetAsync("/api/journal-entries");
        var entries = await entriesResponse.Content.ReadFromJsonAsync<List<JournalEntryDto>>();
        var draft = entries!.First(e => e.Status == "Draft");

        var postResponse = await client.PostAsync($"/api/journal-entries/{draft.Id}/post", null);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/journal-entries/{draft.Id}", new UpdateJournalEntryDto
        {
            OwnerId = draft.OwnerId,
            Lines = draft.Lines.Select(l => new CreateJournalEntryLineDto
            {
                LedgerAccountId = l.LedgerAccountId, Debit = l.Debit, Credit = l.Credit, DisplayOrder = l.DisplayOrder
            }).ToList()
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

        var voidResponse = await client.PostAsync($"/api/journal-entries/{draft.Id}/void", null);
        Assert.Equal(HttpStatusCode.OK, voidResponse.StatusCode);
        var voided = await voidResponse.Content.ReadFromJsonAsync<JournalEntryDto>();
        Assert.Equal("Voided", voided!.Status);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var ar = accounts!.First(a => a.Code == "1100");
        var revenue = accounts!.First(a => a.Code == "4000");
        // Back down to just the seeded Sent CustomerInvoice's 189,000 baseline, not zero.
        Assert.Equal(189000m, ar.Balance);
        // Revenue also carries the seeded POS module's net -180 credit (see PosSaleEndpointTests).
        Assert.Equal(-(189000m + 180m), revenue.Balance);
    }

    [Fact]
    public async Task Get_JournalEntries_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/journal-entries");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
