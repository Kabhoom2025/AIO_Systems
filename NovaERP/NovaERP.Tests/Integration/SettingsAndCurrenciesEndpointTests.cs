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
/// Postgres instance is required to exercise the new Settings/Currency endpoints.</summary>
public class SettingsAndCurrenciesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SettingsAndCurrenciesEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task Get_Settings_Returns_Ok_With_Seeded_Defaults_For_Authenticated_Admin()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<OrganizationSettingsDto>();
        Assert.NotNull(dto);
        Assert.Equal("INR", dto!.DefaultCurrencyCode);
        Assert.Equal("en", dto.DefaultLanguageCode);
    }

    [Fact]
    public async Task Get_Currencies_Returns_Seeded_Reference_Data_For_Authenticated_Admin()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client));

        var response = await client.GetAsync("/api/currencies");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var currencies = await response.Content.ReadFromJsonAsync<List<CurrencyDto>>();
        Assert.NotNull(currencies);
        Assert.Contains(currencies!, c => c.Code == "INR");
        Assert.True(currencies!.Count >= 10);
    }

    [Fact]
    public async Task Get_Settings_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
