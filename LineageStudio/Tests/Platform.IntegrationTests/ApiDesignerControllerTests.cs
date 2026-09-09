using System.Net;
using System.Net.Http.Json;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

public class ApiDesignerControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiDesignerControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateApplicationAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest(name, null));
        var app = await response.Content.ReadFromJsonAsync<ApplicationDto>();
        return app!.Id;
    }

    private async Task<TableDto> CreateTableAsync(Guid appId, string name)
    {
        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables",
            new CreateTableRequest(name, new List<ColumnDefinition> { new("id", ColumnDataType.Uuid, IsPrimaryKey: true) }),
            ApiFactory.JsonOptions);
        return (await response.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions))!;
    }

    [Fact]
    public async Task Create_service_links_to_a_real_table_in_the_same_application()
    {
        var appId = await CreateApplicationAsync("Service App");
        var table = await CreateTableAsync(appId, "customer");

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/services",
            new CreateServiceRequest("CustomerService", table.Id, "Handles customer records"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var service = await response.Content.ReadFromJsonAsync<ServiceDto>();
        Assert.Equal(table.Id, service!.TableId);
    }

    [Fact]
    public async Task Create_service_rejects_a_table_from_another_application()
    {
        var appId = await CreateApplicationAsync("Service App A");
        var otherAppId = await CreateApplicationAsync("Service App B");
        var otherTable = await CreateTableAsync(otherAppId, "foreign_table");

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/services",
            new CreateServiceRequest("CustomerService", otherTable.Id, null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_duplicate_service_name_returns_conflict()
    {
        var appId = await CreateApplicationAsync("Dup Service App");
        await _client.PostAsJsonAsync($"/api/applications/{appId}/services", new CreateServiceRequest("Svc", null, null));

        var second = await _client.PostAsJsonAsync($"/api/applications/{appId}/services", new CreateServiceRequest("Svc", null, null));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_api_endpoint_links_service_and_table()
    {
        var appId = await CreateApplicationAsync("Api App");
        var table = await CreateTableAsync(appId, "customer");
        var serviceResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/services",
            new CreateServiceRequest("CustomerService", table.Id, null));
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var request = new CreateApiEndpointRequest(
            ApiHttpMethod.Post, "/customer",
            RequestSchemaJson: """{"name":"string","email":"string"}""",
            ResponseSchemaJson: """{"id":"uuid"}""",
            ServiceId: service!.Id,
            TableId: table.Id);

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var api = await response.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);
        Assert.Equal("/customer", api!.Path);
        Assert.Equal(service.Id, api.ServiceId);
    }

    [Fact]
    public async Task Create_api_endpoint_normalizes_path_and_rejects_duplicate_method_and_path()
    {
        var appId = await CreateApplicationAsync("Duplicate Api App");

        var first = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Get, "customer", null, null, null, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var firstApi = await first.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);
        Assert.Equal("/customer", firstApi!.Path);

        var duplicate = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Get, "/customer", null, null, null, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // Same path, different method must be allowed.
        var differentMethod = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, null, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, differentMethod.StatusCode);
    }

    [Fact]
    public async Task Create_api_endpoint_rejects_malformed_schema_json()
    {
        var appId = await CreateApplicationAsync("Bad Schema App");

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/widget", "{not json", null, null, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_and_delete_api_endpoint_work()
    {
        var appId = await CreateApplicationAsync("Crud Api App");
        var create = await _client.PostAsJsonAsync($"/api/applications/{appId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Get, "/widget", null, null, null, null), ApiFactory.JsonOptions);
        var api = await create.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        var update = await _client.PutAsJsonAsync($"/api/applications/{appId}/apis/{api!.Id}",
            new UpdateApiEndpointRequest(ApiHttpMethod.Put, "/widget/{id}", null, null, null, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);
        Assert.Equal("/widget/{id}", updated!.Path);

        var delete = await _client.DeleteAsync($"/api/applications/{appId}/apis/{api.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/applications/{appId}/apis/{api.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
