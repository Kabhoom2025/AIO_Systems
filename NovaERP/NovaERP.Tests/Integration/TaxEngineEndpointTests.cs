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
/// Postgres instance is required to exercise the new Tax Code endpoints.</summary>
public class TaxEngineEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TaxEngineEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_TaxCodes_Returns_Seeded_Compound_And_Zero_Rate_Codes_For_Authenticated_Admin()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/tax-codes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var taxCodes = await response.Content.ReadFromJsonAsync<List<TaxCodeDto>>();
        Assert.NotNull(taxCodes);

        var gst18 = Assert.Single(taxCodes!, t => t.Code == "GST18");
        Assert.Equal(2, gst18.Components.Count);
        Assert.Equal(18m, gst18.TotalRatePercent);

        var zero = Assert.Single(taxCodes!, t => t.Code == "ZERO");
        Assert.Equal(0m, zero.TotalRatePercent);
    }

    [Fact]
    public async Task Post_TaxCode_Creates_Compound_Tax_Code_And_Computes_Total_Rate()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.PostAsJsonAsync("/api/tax-codes", new CreateTaxCodeDto
        {
            Code = "IGST18",
            Name = "IGST 18%",
            IsActive = true,
            Components = new List<CreateTaxComponentDto>
            {
                new() { Name = "IGST", RatePercent = 18m, DisplayOrder = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaxCodeDto>();
        Assert.NotNull(created);
        Assert.Equal("IGST18", created!.Code);
        Assert.Equal(18m, created.TotalRatePercent);
    }

    [Fact]
    public async Task Post_TaxCode_With_Duplicate_Code_Returns_BadRequest()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.PostAsJsonAsync("/api/tax-codes", new CreateTaxCodeDto
        {
            Code = "GST18",
            Name = "Duplicate GST 18%",
            IsActive = true,
            Components = new List<CreateTaxComponentDto>
            {
                new() { Name = "CGST", RatePercent = 9m, DisplayOrder = 1 },
                new() { Name = "SGST", RatePercent = 9m, DisplayOrder = 2 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_TaxCodes_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tax-codes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
