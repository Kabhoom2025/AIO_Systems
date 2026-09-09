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

/// <summary>Exercises the Customer Portal's self-service scoping — every endpoint here is
/// reachable with plain [Authorize] (no crm.view policy), and every query is scoped to the
/// caller's own linked Contact record via Contact.UserId, not a permission check. Priya Nair
/// is seeded linked to customer@novaerp.local specifically to exercise this live.</summary>
public class CustomerPortalEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CustomerPortalEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task GetMyProfile_Returns_Own_Contact_And_Account()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "customer@novaerp.local", "Customer@123"));

        var response = await client.GetAsync("/api/my-account/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<MyContactDto>();
        Assert.Equal("Priya", profile!.FirstName);
        Assert.Equal("Nair", profile.LastName);
        Assert.Equal("Globex Retail", profile.AccountName);
    }

    [Fact]
    public async Task GetMyProfile_Returns_404_When_No_Contact_Linked()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync("/api/my-account/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_Returns_Only_My_Own_Accounts_Orders()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "customer@novaerp.local", "Customer@123"));

        var response = await client.GetAsync("/api/my-account/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<SalesOrderDto>>();
        Assert.NotEmpty(orders!);
        Assert.All(orders!, o => Assert.Equal("Globex Retail", o.AccountName));
        Assert.DoesNotContain(orders!, o => o.AccountName == "Acme Manufacturing");
    }

    [Fact]
    public async Task GetMyInvoices_Returns_Only_My_Own_Accounts_Invoices()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "customer@novaerp.local", "Customer@123"));

        var response = await client.GetAsync("/api/my-account/invoices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invoices = await response.Content.ReadFromJsonAsync<List<CustomerInvoiceDto>>();
        Assert.NotEmpty(invoices!);
        Assert.All(invoices!, i => Assert.Equal("Globex Retail", i.AccountName));
        Assert.DoesNotContain(invoices!, i => i.AccountName == "Acme Manufacturing");
    }

    [Fact]
    public async Task GetMyProfile_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/my-account/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
