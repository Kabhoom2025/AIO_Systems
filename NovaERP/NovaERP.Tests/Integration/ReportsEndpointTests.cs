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

/// <summary>Exercises the Reports module — every report is a pure in-memory aggregation over
/// an existing admin service's own GetAllAsync result, gated by reports.view (a real
/// Admin/Manager-facing permission, unlike Dashboard Builder's self-service surface, since
/// Trial Balance exposes Finance data). reports is deliberately not a foundationModule, so
/// employee@novaerp.local should be rejected.</summary>
public class ReportsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReportsEndpointTests(WebApplicationFactory<Program> factory) =>
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
    public async Task GetTrialBalance_Returns_Balanced_Debit_And_Credit_Totals()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync("/api/reports/trial-balance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<TrialBalanceDto>();
        Assert.NotEmpty(report!.Lines);
        // A correctly posted General Ledger always balances — the fundamental accounting
        // identity, robust to any future change in seeded journal entries.
        Assert.Equal(report.TotalDebit, report.TotalCredit);
        Assert.True(report.TotalDebit > 0);
    }

    [Fact]
    public async Task GetSalesOrderSummary_Groups_By_Status_And_Totals_Match()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var ordersResponse = await client.GetAsync("/api/sales-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<SalesOrderDto>>();

        var response = await client.GetAsync("/api/reports/sales-order-summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<StatusSummaryReportDto>();
        Assert.NotEmpty(report!.Rows);
        Assert.Equal(orders!.Count, report.Rows.Sum(r => r.Count));
        Assert.Equal(orders.Sum(o => o.GrandTotal), report.GrandTotal);
    }

    [Fact]
    public async Task GetPurchaseOrderSummary_Groups_By_Status_And_Totals_Match()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var ordersResponse = await client.GetAsync("/api/purchase-orders");
        var orders = await ordersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();

        var response = await client.GetAsync("/api/reports/purchase-order-summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<StatusSummaryReportDto>();
        Assert.NotEmpty(report!.Rows);
        Assert.Equal(orders!.Count, report.Rows.Sum(r => r.Count));
        Assert.Equal(orders.Sum(o => o.GrandTotal), report.GrandTotal);
    }

    [Fact]
    public async Task Employee_Without_ReportsView_Is_Forbidden()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/reports/trial-balance");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTrialBalance_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/reports/trial-balance");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
