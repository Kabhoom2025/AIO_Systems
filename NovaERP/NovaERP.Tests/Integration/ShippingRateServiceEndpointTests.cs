using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;
using NovaERP.Infrastructure.Services;
using NovaERP.Tests.TestDoubles;
using Xunit;

namespace NovaERP.Tests.Integration;

/// <summary>Exercises the full Shipping Rate Connector engine end to end through
/// GET /api/shipments/{id}/rates: a real saved connector + field mappings builds a real
/// outbound request JSON from a real Shipment/Package/Warehouse, sends it through a fake HTTP
/// handler standing in for the external TMS, and parses the fake TMS's canned response back
/// into normalized RateQuoteDtos — proving the mapping engine actually works, not just that the
/// CRUD screens compile.</summary>
public class ShippingRateServiceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly FakeRateShopHttpMessageHandler _fakeHandler = new();

    public ShippingRateServiceEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient(nameof(ShippingConnectorHttpRunner))
                    .ConfigurePrimaryHttpMessageHandler(() => _fakeHandler);
            });
        });

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    private async Task<int> SeedShipmentWithConnectorAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);

        var warehouse = await db.Warehouses.FirstAsync(w => w.Code == "WH-BLR");
        warehouse.PostalCode = "560001";
        warehouse.Country = "India";

        var adminUser = await db.Users.FirstAsync(u => u.Email == "admin@novaerp.local");

        var shipment = new Shipment
        {
            OrganizationId = warehouse.OrganizationId,
            ShipmentNumber = "TEST-RATE-001",
            WarehouseId = warehouse.Id,
            SourceType = "TransferOrder",
            ShipToPostalCode = "110001",
            ShipToCountry = "India",
            OwnerId = adminUser.Id,
            Packages = new List<ShipmentPackage>
            {
                new() { PackageNumber = 1, WeightKg = 2.5m }
            }
        };
        db.Shipments.Add(shipment);

        var connector = new ShippingConnector
        {
            OrganizationId = warehouse.OrganizationId,
            Name = "Fake Test TMS",
            TmsType = "FakeTms",
            BaseUrl = "https://fake-tms.test/rates",
            HttpMethod = "POST",
            AuthType = "None",
            IsActive = true,
            FieldMappings = new List<ShippingConnectorFieldMapping>
            {
                new() { Direction = "Outbound", NovaField = "Shipment.ShipToPostalCode", ExternalPath = "destination.zip", Transform = "None" },
                new() { Direction = "Outbound", NovaField = "Package.WeightKg", ExternalPath = "packages[].weight", Transform = "None" },
                new() { Direction = "Inbound", NovaField = "Quote.CarrierName", ExternalPath = "rates[].carrier", Transform = "None" },
                new() { Direction = "Inbound", NovaField = "Quote.Price", ExternalPath = "rates[].price", Transform = "None" }
            }
        };
        db.ShippingConnectors.Add(connector);

        await db.SaveChangesAsync();
        return shipment.Id;
    }

    [Fact]
    public async Task GetRates_Builds_Request_From_Shipment_And_Parses_The_Fake_TMS_Response()
    {
        _fakeHandler.ResponseJson = """
            {"rates":[{"carrier":"BlueDart","price":120.50},{"carrier":"DTDC","price":95.0}]}
            """;

        var shipmentId = await SeedShipmentWithConnectorAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync($"/api/shipments/{shipmentId}/rates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RateQuoteResultDto>();
        Assert.Empty(result!.Errors);
        Assert.Equal(2, result.Quotes.Count);

        Assert.Equal("BlueDart", result.Quotes[0].CarrierName);
        Assert.Equal(120.50m, result.Quotes[0].Price);
        Assert.Equal("DTDC", result.Quotes[1].CarrierName);
        Assert.Equal(95.0m, result.Quotes[1].Price);
        Assert.All(result.Quotes, q => Assert.Equal("Fake Test TMS", q.ConnectorName));

        // Prove the OUTBOUND mapping built a real request, not just that the inbound side parses.
        Assert.Contains("\"110001\"", _fakeHandler.LastRequestBody);
        Assert.Contains("2.5", _fakeHandler.LastRequestBody);
    }

    [Fact]
    public async Task GetRates_With_No_Active_Connectors_Returns_An_Explanatory_Error()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
            await db.Database.EnsureCreatedAsync();
            await SeedData.SeedAsync(db);
        }

        var shipmentId = await SeedShipmentWithConnectorAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
            var connector = await db.ShippingConnectors.FirstAsync(c => c.Name == "Fake Test TMS");
            connector.IsActive = false;
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var response = await client.GetAsync($"/api/shipments/{shipmentId}/rates");
        var result = await response.Content.ReadFromJsonAsync<RateQuoteResultDto>();

        Assert.Empty(result!.Quotes);
        Assert.Single(result.Errors);
    }
}
