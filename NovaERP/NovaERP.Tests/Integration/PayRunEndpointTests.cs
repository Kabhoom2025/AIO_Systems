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

/// <summary>Exercises the PayRun lifecycle (Draft -> Processed -> Paid, or Cancelled from
/// Draft/Processed) and the GL posting PayAsync performs — the module's core business logic
/// generalizing VendorBill's Approve+Pay pattern into a single balanced 3-line entry.</summary>
public class PayRunEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PayRunEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_PayRuns_Returns_Seeded_Paid_And_Draft_Runs()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/pay-runs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var runs = await response.Content.ReadFromJsonAsync<List<PayRunDto>>();
        Assert.NotNull(runs);
        Assert.Equal(2, runs!.Count);

        var paid = Assert.Single(runs!, r => r.Status == "Paid");
        Assert.Equal(4, paid.Lines.Count);
        Assert.Equal(274000m, paid.TotalGrossPay);
        Assert.Equal(27400m, paid.TotalDeductions);
        Assert.Equal(246600m, paid.TotalNetPay);
        Assert.NotNull(paid.PostedJournalEntryId);

        var draft = Assert.Single(runs!, r => r.Status == "Draft");
        Assert.Empty(draft.Lines);
    }

    [Fact]
    public async Task Process_Then_Pay_Posts_Correct_Balanced_JournalEntry()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var runsResponse = await client.GetAsync("/api/pay-runs");
        var runs = await runsResponse.Content.ReadFromJsonAsync<List<PayRunDto>>();
        var draft = runs!.First(r => r.Status == "Draft");

        var processResponse = await client.PostAsync($"/api/pay-runs/{draft.Id}/process", null);
        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);
        var processed = await processResponse.Content.ReadFromJsonAsync<PayRunDto>();
        Assert.Equal("Processed", processed!.Status);
        Assert.Equal(4, processed.Lines.Count);
        Assert.Equal(274000m, processed.TotalGrossPay);
        Assert.Equal(246600m, processed.TotalNetPay);

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");
        var cashBeforePay = cash.Balance;
        var salaryExpenseBefore = accounts!.First(a => a.Code == "5100").Balance;
        var salariesPayableBefore = accounts!.First(a => a.Code == "2100").Balance;

        var payResponse = await client.PostAsJsonAsync($"/api/pay-runs/{draft.Id}/pay", new PayPayRunDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
        var paid = await payResponse.Content.ReadFromJsonAsync<PayRunDto>();
        Assert.Equal("Paid", paid!.Status);
        Assert.NotNull(paid.PostedJournalEntryId);

        var accountsAfterResponse = await client.GetAsync("/api/ledger-accounts");
        var accountsAfter = await accountsAfterResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        Assert.Equal(cashBeforePay - 246600m, accountsAfter!.First(a => a.Code == "1000").Balance);
        Assert.Equal(salaryExpenseBefore + 274000m, accountsAfter!.First(a => a.Code == "5100").Balance);
        Assert.Equal(salariesPayableBefore - 27400m, accountsAfter!.First(a => a.Code == "2100").Balance);
    }

    [Fact]
    public async Task Pay_Requires_Processed_And_Cancel_Not_Allowed_After_Paid()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var runsResponse = await client.GetAsync("/api/pay-runs");
        var runs = await runsResponse.Content.ReadFromJsonAsync<List<PayRunDto>>();
        var draft = runs!.First(r => r.Status == "Draft");

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var cash = accounts!.First(a => a.Code == "1000");

        var payBeforeProcessResponse = await client.PostAsJsonAsync($"/api/pay-runs/{draft.Id}/pay", new PayPayRunDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.BadRequest, payBeforeProcessResponse.StatusCode);

        await client.PostAsync($"/api/pay-runs/{draft.Id}/process", null);
        var payResponse = await client.PostAsJsonAsync($"/api/pay-runs/{draft.Id}/pay", new PayPayRunDto { PaymentLedgerAccountId = cash.Id });
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        var cancelResponse = await client.PostAsync($"/api/pay-runs/{draft.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/pay-runs/{draft.Id}", new UpdatePayRunDto
        {
            PeriodMonth = draft.PeriodMonth, PeriodYear = draft.PeriodYear,
            ExpenseLedgerAccountId = draft.ExpenseLedgerAccountId,
            DeductionsPayableLedgerAccountId = draft.DeductionsPayableLedgerAccountId, OwnerId = draft.OwnerId
        });
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_Works_From_Draft_And_From_Processed()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var usersResponse = await client.GetAsync("/api/users");
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        var owner = users!.First();

        var accountsResponse = await client.GetAsync("/api/ledger-accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<LedgerAccountDto>>();
        var expenseAccount = accounts!.First(a => a.Code == "5100");
        var payableAccount = accounts!.First(a => a.Code == "2100");

        async Task<PayRunDto> CreateRunAsync(int month, int year)
        {
            var response = await client.PostAsJsonAsync("/api/pay-runs", new CreatePayRunDto
            {
                PeriodMonth = month, PeriodYear = year,
                ExpenseLedgerAccountId = expenseAccount.Id, DeductionsPayableLedgerAccountId = payableAccount.Id,
                OwnerId = owner.Id
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<PayRunDto>())!;
        }

        var draftRun = await CreateRunAsync(1, 2020);
        var cancelDraftResponse = await client.PostAsync($"/api/pay-runs/{draftRun.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelDraftResponse.StatusCode);

        var processedRun = await CreateRunAsync(2, 2020);
        await client.PostAsync($"/api/pay-runs/{processedRun.Id}/process", null);
        var cancelProcessedResponse = await client.PostAsync($"/api/pay-runs/{processedRun.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelProcessedResponse.StatusCode);
        var cancelled = await cancelProcessedResponse.Content.ReadFromJsonAsync<PayRunDto>();
        Assert.Equal("Cancelled", cancelled!.Status);
    }

    [Fact]
    public async Task Get_PayRuns_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/pay-runs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
