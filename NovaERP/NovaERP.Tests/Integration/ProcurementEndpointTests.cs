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
/// Postgres instance is required to exercise the new Vendor/RFQ endpoints.</summary>
public class ProcurementEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProcurementEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Vendors_Returns_Seeded_Vendors_For_Authenticated_Admin()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/vendors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vendors = await response.Content.ReadFromJsonAsync<List<VendorDto>>();
        Assert.NotNull(vendors);
        Assert.Contains(vendors!, v => v.Name == "Bharat Steel Traders");
        Assert.True(vendors!.Count >= 2);
    }

    [Fact]
    public async Task Get_Rfqs_Returns_Seeded_Sent_Rfq_With_One_Quote_Recorded_And_One_Pending()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/rfqs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rfqs = await response.Content.ReadFromJsonAsync<List<RfqRequestDto>>();
        Assert.NotNull(rfqs);

        var sent = Assert.Single(rfqs!, r => r.Status == "Sent");
        Assert.Equal(2, sent.Quotes.Count);
        Assert.Contains(sent.Quotes, q => q.QuotedAmount == 425000m);
        Assert.Contains(sent.Quotes, q => q.QuotedAmount == null);
    }

    [Fact]
    public async Task RecordQuote_From_Uninvited_Vendor_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var rfqsResponse = await client.GetAsync("/api/rfqs");
        var rfqs = await rfqsResponse.Content.ReadFromJsonAsync<List<RfqRequestDto>>();
        var sentRfq = rfqs!.First(r => r.Status == "Sent");

        // Create a fresh vendor guaranteed not to be among this RFQ's invited quotes.
        var newVendorResponse = await client.PostAsJsonAsync("/api/vendors", new CreateVendorDto
        {
            Name = "Uninvited Test Vendor",
            OwnerId = sentRfq.OwnerId
        });
        var uninvitedVendor = await newVendorResponse.Content.ReadFromJsonAsync<VendorDto>();

        var response = await client.PostAsJsonAsync($"/api/rfqs/{sentRfq.Id}/record-quote", new RecordRfqQuoteDto
        {
            VendorId = uninvitedVendor!.Id,
            QuotedAmount = 1000m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Send_RecordQuote_Then_Close_Full_Flow_Succeeds()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var rfqsResponse = await client.GetAsync("/api/rfqs");
        var rfqs = await rfqsResponse.Content.ReadFromJsonAsync<List<RfqRequestDto>>();
        var draftRfq = rfqs!.First(r => r.Status == "Draft");

        var sendResponse = await client.PostAsync($"/api/rfqs/{draftRfq.Id}/send", null);
        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var sent = await sendResponse.Content.ReadFromJsonAsync<RfqRequestDto>();
        Assert.Equal("Sent", sent!.Status);

        var invitedVendorId = sent.Quotes.First().VendorId;
        var quoteResponse = await client.PostAsJsonAsync($"/api/rfqs/{draftRfq.Id}/record-quote", new RecordRfqQuoteDto
        {
            VendorId = invitedVendorId,
            QuotedAmount = 50000m,
            Notes = "Best price"
        });
        Assert.Equal(HttpStatusCode.OK, quoteResponse.StatusCode);

        var closeResponse = await client.PostAsJsonAsync($"/api/rfqs/{draftRfq.Id}/close", new CloseRfqRequestDto
        {
            WinningVendorId = invitedVendorId
        });
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
        var closed = await closeResponse.Content.ReadFromJsonAsync<RfqRequestDto>();
        Assert.Equal("Closed", closed!.Status);
        Assert.Equal(invitedVendorId, closed.WinningVendorId);
    }

    [Fact]
    public async Task Update_Non_Draft_Rfq_Is_Rejected()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var rfqsResponse = await client.GetAsync("/api/rfqs");
        var rfqs = await rfqsResponse.Content.ReadFromJsonAsync<List<RfqRequestDto>>();
        var sentRfq = rfqs!.First(r => r.Status == "Sent");

        var response = await client.PutAsJsonAsync($"/api/rfqs/{sentRfq.Id}", new UpdateRfqRequestDto
        {
            Title = "Should fail",
            OwnerId = sentRfq.OwnerId,
            Items = { new CreateRfqItemDto { ItemName = "X", Quantity = 1m } },
            InvitedVendorIds = { sentRfq.Quotes.First().VendorId }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Vendors_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vendors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
