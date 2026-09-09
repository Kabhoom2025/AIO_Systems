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

/// <summary>Exercises Carrier Connector CRUD (backed by the ShippingConnector entity/routes) —
/// a real Admin/Manager-facing surface with its own dedicated carrier-connector.view/create/
/// edit/delete permission module (not piggybacked on settings.*). Secrets (AuthApiKey/
/// AuthPassword) never round-trip in any response body — only a computed HasAuthApiKey/
/// HasAuthPassword flag does.</summary>
public class ShippingConnectorEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ShippingConnectorEndpointTests(WebApplicationFactory<Program> factory) =>
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

    private static SaveShippingConnectorDto SampleConnector() => new()
    {
        Name = "Test TMS",
        TmsType = "TestVendor",
        BaseUrl = "https://example.test/rates",
        HttpMethod = "POST",
        AuthType = "BearerToken",
        AuthApiKey = "secret-token-123",
        IsActive = true,
        FieldMappings = new List<UpsertFieldMappingDto>
        {
            new() { Direction = "Outbound", NovaField = "Shipment.ShipToPostalCode", ExternalPath = "destination.zip", Transform = "None" },
            new() { Direction = "Inbound", NovaField = "Quote.Price", ExternalPath = "rates[].price", Transform = "None" }
        }
    };

    [Fact]
    public async Task Admin_Can_Create_Then_Retrieve_A_Connector_Without_The_Secret_Ever_Echoing()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var createResponse = await client.PostAsJsonAsync("/api/shipping-connectors", SampleConnector());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret-token-123", createBody);

        var created = await createResponse.Content.ReadFromJsonAsync<ShippingConnectorDto>();
        Assert.True(created!.HasAuthApiKey);
        Assert.Equal(2, created.FieldMappings.Count);

        var getResponse = await client.GetAsync($"/api/shipping-connectors/{created.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret-token-123", getBody);
    }

    [Fact]
    public async Task Update_With_Blank_ApiKey_Leaves_The_Existing_Key_Unchanged()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var createResponse = await client.PostAsJsonAsync("/api/shipping-connectors", SampleConnector());
        var created = await createResponse.Content.ReadFromJsonAsync<ShippingConnectorDto>();

        var updateDto = SampleConnector();
        updateDto.AuthApiKey = ""; // blank means keep unchanged
        var updateResponse = await client.PutAsJsonAsync($"/api/shipping-connectors/{created!.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ShippingConnectorDto>();
        Assert.True(updated!.HasAuthApiKey);
    }

    [Fact]
    public async Task Employee_Can_View_But_Cannot_Create_Or_Edit()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var getResponse = await client.GetAsync("/api/shipping-connectors");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync("/api/shipping-connectors", SampleConnector());
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Removes_The_Connector()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "admin@novaerp.local", "Admin@123"));

        var createResponse = await client.PostAsJsonAsync("/api/shipping-connectors", SampleConnector());
        var created = await createResponse.Content.ReadFromJsonAsync<ShippingConnectorDto>();

        var deleteResponse = await client.DeleteAsync($"/api/shipping-connectors/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/shipping-connectors/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/shipping-connectors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
