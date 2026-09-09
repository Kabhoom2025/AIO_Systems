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

/// <summary>Exercises the Vendor Portal's self-service scoping — every endpoint here is
/// reachable with plain [Authorize] (no procurement.view/purchase.view policy), and every
/// query is scoped to the caller's own linked Vendor record via Vendor.UserId, not a
/// permission check. Bharat Steel Traders is seeded linked to vendor@novaerp.local
/// specifically to exercise this live.</summary>
public class VendorPortalEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public VendorPortalEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task GetMyProfile_Returns_Own_Vendor_Record()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "vendor@novaerp.local", "Vendor@123"));

        var response = await client.GetAsync("/api/vendor-portal/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<MyVendorDto>();
        Assert.Equal("Bharat Steel Traders", profile!.Name);
    }

    [Fact]
    public async Task GetMyProfile_Returns_404_When_No_Vendor_Linked()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync("/api/vendor-portal/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyPurchaseOrders_Returns_Only_My_Own_Vendors_Orders()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "vendor@novaerp.local", "Vendor@123"));

        var response = await client.GetAsync("/api/vendor-portal/purchase-orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
        Assert.NotEmpty(orders!);
        Assert.All(orders!, o => Assert.Equal("Bharat Steel Traders", o.VendorName));
        Assert.DoesNotContain(orders!, o => o.VendorName == "SafePack Industries");
    }

    [Fact]
    public async Task GetMyBills_Returns_Only_My_Own_Vendors_Bills()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "vendor@novaerp.local", "Vendor@123"));

        var response = await client.GetAsync("/api/vendor-portal/bills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bills = await response.Content.ReadFromJsonAsync<List<VendorBillDto>>();
        Assert.NotEmpty(bills!);
        Assert.All(bills!, b => Assert.Equal("Bharat Steel Traders", b.VendorName));
        Assert.DoesNotContain(bills!, b => b.VendorName == "SafePack Industries");
    }

    [Fact]
    public async Task GetMyProfile_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/vendor-portal/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
