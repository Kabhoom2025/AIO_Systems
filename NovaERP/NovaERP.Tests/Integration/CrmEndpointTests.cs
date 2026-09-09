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
/// Postgres instance is required to exercise the new CRM endpoints.</summary>
public class CrmEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CrmEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Accounts_Returns_Seeded_Accounts_For_Authenticated_Admin()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountDto>>();
        Assert.NotNull(accounts);
        Assert.Contains(accounts!, a => a.Name == "Acme Manufacturing");
        Assert.True(accounts!.Count >= 2);
    }

    [Fact]
    public async Task Get_Leads_Returns_Seeded_Leads_Including_A_Converted_One()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/leads");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var leads = await response.Content.ReadFromJsonAsync<List<LeadDto>>();
        Assert.NotNull(leads);

        var converted = Assert.Single(leads!, l => l.Status == "Converted");
        Assert.NotNull(converted.ConvertedAccountId);
        Assert.Equal("Acme Manufacturing", converted.ConvertedAccountName);
    }

    [Fact]
    public async Task Post_Lead_Convert_Creates_Account_Contact_And_Opportunity()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var leadsResponse = await client.GetAsync("/api/leads");
        var leads = await leadsResponse.Content.ReadFromJsonAsync<List<LeadDto>>();
        var newLead = leads!.First(l => l.Status == "New");

        var convertResponse = await client.PostAsJsonAsync($"/api/leads/{newLead.Id}/convert", new ConvertLeadDto
        {
            AccountName = "Kumar Textiles Pvt Ltd",
            CreateOpportunity = true,
            OpportunityName = "Kumar Textiles — Initial Order",
            OpportunityAmount = 50000m
        });

        Assert.Equal(HttpStatusCode.OK, convertResponse.StatusCode);
        var result = await convertResponse.Content.ReadFromJsonAsync<ConvertLeadResultDto>();
        Assert.NotNull(result);
        Assert.True(result!.AccountId > 0);
        Assert.True(result.ContactId > 0);
        Assert.NotNull(result.OpportunityId);

        var updatedLeadResponse = await client.GetAsync($"/api/leads/{newLead.Id}");
        var updatedLead = await updatedLeadResponse.Content.ReadFromJsonAsync<LeadDto>();
        Assert.Equal("Converted", updatedLead!.Status);
        Assert.Equal(result.AccountId, updatedLead.ConvertedAccountId);

        var accountResponse = await client.GetAsync($"/api/accounts/{result.AccountId}");
        Assert.Equal(HttpStatusCode.OK, accountResponse.StatusCode);
        var account = await accountResponse.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal("Kumar Textiles Pvt Ltd", account!.Name);
    }

    [Fact]
    public async Task Post_Lead_Convert_On_Already_Converted_Lead_Returns_BadRequest()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var leadsResponse = await client.GetAsync("/api/leads");
        var leads = await leadsResponse.Content.ReadFromJsonAsync<List<LeadDto>>();
        var convertedLead = leads!.First(l => l.Status == "Converted");

        var response = await client.PostAsJsonAsync($"/api/leads/{convertedLead.Id}/convert", new ConvertLeadDto());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Opportunity_Stage_To_Won_Succeeds()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var oppsResponse = await client.GetAsync("/api/opportunities");
        var opportunities = await oppsResponse.Content.ReadFromJsonAsync<List<OpportunityDto>>();
        var opp = opportunities!.First();

        var response = await client.PutAsJsonAsync($"/api/opportunities/{opp.Id}", new UpdateOpportunityDto
        {
            Name = opp.Name,
            Amount = opp.Amount,
            Stage = "Won",
            OwnerId = opp.OwnerId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<OpportunityDto>();
        Assert.Equal("Won", updated!.Stage);
    }

    [Fact]
    public async Task Get_Accounts_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
