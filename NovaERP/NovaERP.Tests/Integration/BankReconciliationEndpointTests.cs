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

/// <summary>Exercises BankReconciliationService's Match/Complete actions — Complete's live
/// comparison of StatementEndingBalance against the LedgerAccount's actual balance is the
/// module's one genuinely new invariant (a DB-backed check, mirroring StockTransferService's
/// on-hand check, not a pure-DTO validator rule).</summary>
public class BankReconciliationEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BankReconciliationEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_BankReconciliations_Returns_Seeded_Completed_And_Draft_With_Correct_Computed_Fields()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/bank-reconciliations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reconciliations = await response.Content.ReadFromJsonAsync<List<BankReconciliationDto>>();
        Assert.NotNull(reconciliations);

        var completed = Assert.Single(reconciliations!, r => r.Status == "Completed");
        Assert.Equal(0, completed.UnmatchedCount);
        // BookBalance is always live, not a frozen snapshot — this reconciliation's statement
        // balance (500,000) predates both the later-seeded 5,000 bank-interest entry and the
        // Payroll module's 246,600 net-pay credit (PR-00001), plus the POS module's net +180,
        // so the live book balance (258,580) now differs from it by exactly that amount.
        Assert.Equal(241420m, completed.Difference);

        var draft = Assert.Single(reconciliations!, r => r.Status == "Draft");
        Assert.Equal(1, draft.UnmatchedCount);
        Assert.Equal(258580m, draft.BookBalance);
        Assert.Equal(0m, draft.Difference); // StatementEndingBalance (258,580) already equals book balance
    }

    [Fact]
    public async Task Match_Line_With_Amount_Mismatch_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        // A deliberately mismatched statement line amount (100) — the only unmatched Posted
        // candidate for Cash is the seeded 5,000 bank-interest line (the 500,000 opening line
        // is already matched by the seeded Completed reconciliation).
        var createResponse = await client.PostAsJsonAsync("/api/bank-reconciliations", new CreateBankReconciliationDto
        {
            LedgerAccountId = cash.Id,
            StatementEndingBalance = cash.Balance,
            OwnerId = owner.Id,
            Lines = new List<CreateBankStatementLineDto> { new() { Amount = 100m, DisplayOrder = 1 } }
        });
        var created = await createResponse.Content.ReadFromJsonAsync<BankReconciliationDto>();
        var line = created!.Lines.Single();

        var candidatesResponse = await client.GetAsync($"/api/bank-reconciliations/match-candidates?ledgerAccountId={cash.Id}");
        var candidates = await candidatesResponse.Content.ReadFromJsonAsync<List<MatchCandidateDto>>();
        var mismatchedCandidate = candidates!.First(c => c.Amount == 5000m);

        var matchResponse = await client.PostAsJsonAsync(
            $"/api/bank-reconciliations/{created.Id}/lines/{line.Id}/match",
            new MatchBankStatementLineDto { JournalEntryLineId = mismatchedCandidate.JournalEntryLineId });

        Assert.Equal(HttpStatusCode.BadRequest, matchResponse.StatusCode);
    }

    [Fact]
    public async Task Complete_With_Unmatched_Lines_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var reconciliationsResponse = await client.GetAsync("/api/bank-reconciliations");
        var reconciliations = await reconciliationsResponse.Content.ReadFromJsonAsync<List<BankReconciliationDto>>();
        var draft = reconciliations!.First(r => r.Status == "Draft");

        var completeResponse = await client.PostAsync($"/api/bank-reconciliations/{draft.Id}/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, completeResponse.StatusCode);
    }

    [Fact]
    public async Task Match_Correct_Line_Then_Complete_Succeeds_And_Edit_After_Complete_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var reconciliationsResponse = await client.GetAsync("/api/bank-reconciliations");
        var reconciliations = await reconciliationsResponse.Content.ReadFromJsonAsync<List<BankReconciliationDto>>();
        var draft = reconciliations!.First(r => r.Status == "Draft");
        var line = draft.Lines.Single();

        var candidatesResponse = await client.GetAsync($"/api/bank-reconciliations/match-candidates?ledgerAccountId={draft.LedgerAccountId}");
        var candidates = await candidatesResponse.Content.ReadFromJsonAsync<List<MatchCandidateDto>>();
        var correctCandidate = candidates!.First(c => c.Amount == 5000m);

        var matchResponse = await client.PostAsJsonAsync(
            $"/api/bank-reconciliations/{draft.Id}/lines/{line.Id}/match",
            new MatchBankStatementLineDto { JournalEntryLineId = correctCandidate.JournalEntryLineId });
        Assert.Equal(HttpStatusCode.OK, matchResponse.StatusCode);
        var matched = await matchResponse.Content.ReadFromJsonAsync<BankReconciliationDto>();
        Assert.Equal(0, matched!.UnmatchedCount);

        var completeResponse = await client.PostAsync($"/api/bank-reconciliations/{draft.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completed = await completeResponse.Content.ReadFromJsonAsync<BankReconciliationDto>();
        Assert.Equal("Completed", completed!.Status);
        Assert.Equal(0m, completed.Difference);

        var updateResponse = await client.PutAsJsonAsync($"/api/bank-reconciliations/{draft.Id}", new UpdateBankReconciliationDto
        {
            StatementEndingBalance = draft.StatementEndingBalance,
            OwnerId = draft.OwnerId,
            Lines = new List<CreateBankStatementLineDto> { new() { Amount = 1m, DisplayOrder = 1 } }
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Get_BankReconciliations_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/bank-reconciliations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
