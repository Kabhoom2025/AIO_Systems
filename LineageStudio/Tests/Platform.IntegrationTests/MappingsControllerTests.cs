using System.Net;
using System.Net.Http.Json;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

public class MappingsControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public MappingsControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record Fixture(Guid AppId, Guid ComponentId, Guid ApiId, Guid ServiceId, Guid ColumnId);

    private async Task<Fixture> BuildFixtureAsync()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Mapping App", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var screenResponse = await _client.PostAsJsonAsync($"/api/applications/{app!.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        var componentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
            new CreateComponentRequest(screen!.Id, ComponentType.Input, "CustomerForm.name", 0, 0, DataBinding: "name"),
            ApiFactory.JsonOptions);
        var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);

        var tableResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
                new("name", ColumnDataType.Varchar, Length: 200),
            }), ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);
        var nameColumn = table!.Columns.Single(c => c.Name == "name");

        var serviceResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/services",
            new CreateServiceRequest("CustomerService", table.Id, null));
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var apiResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, service!.Id, table.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        return new Fixture(app.Id, component!.Id, api!.Id, service.Id, nameColumn.Id);
    }

    [Fact]
    public async Task Create_mapping_links_component_api_service_and_column()
    {
        var fx = await BuildFixtureAsync();

        var request = new CreateMappingRequest(
            fx.ComponentId, "name", fx.ApiId, "name", fx.ServiceId, "name", fx.ColumnId,
            TransformationType.Trim, null);

        var response = await _client.PostAsJsonAsync($"/api/applications/{fx.AppId}/mappings", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var mapping = await response.Content.ReadFromJsonAsync<MappingDto>(ApiFactory.JsonOptions);
        Assert.Equal(fx.ColumnId, mapping!.ColumnId);
        Assert.Equal(TransformationType.Trim, mapping.Transformation);
    }

    [Fact]
    public async Task Create_mapping_rejects_a_component_from_another_application()
    {
        var fx = await BuildFixtureAsync();
        var otherAppResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Other Mapping App", null));
        var otherApp = await otherAppResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var request = new CreateMappingRequest(
            fx.ComponentId, "name", fx.ApiId, "name", null, null, fx.ColumnId, TransformationType.None, null);

        var response = await _client.PostAsJsonAsync($"/api/applications/{otherApp!.Id}/mappings", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(TransformationType.Default)]
    [InlineData(TransformationType.Concatenate)]
    [InlineData(TransformationType.Split)]
    [InlineData(TransformationType.DateConversion)]
    [InlineData(TransformationType.NumberConversion)]
    public async Task Create_mapping_requires_config_for_transformations_that_need_it(TransformationType transformation)
    {
        var fx = await BuildFixtureAsync();

        var request = new CreateMappingRequest(
            fx.ComponentId, "name", fx.ApiId, "name", null, null, fx.ColumnId, transformation, null);

        var response = await _client.PostAsJsonAsync($"/api/applications/{fx.AppId}/mappings", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_mapping_with_concatenate_config_succeeds()
    {
        var fx = await BuildFixtureAsync();

        var request = new CreateMappingRequest(
            fx.ComponentId, "name", fx.ApiId, "name", null, null, fx.ColumnId,
            TransformationType.Concatenate, """{"fields":["firstName","lastName"],"separator":" "}""");

        var response = await _client.PostAsJsonAsync($"/api/applications/{fx.AppId}/mappings", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_and_delete_mapping_work()
    {
        var fx = await BuildFixtureAsync();
        var create = await _client.PostAsJsonAsync($"/api/applications/{fx.AppId}/mappings",
            new CreateMappingRequest(fx.ComponentId, "name", fx.ApiId, "name", null, null, fx.ColumnId, TransformationType.None, null),
            ApiFactory.JsonOptions);
        var mapping = await create.Content.ReadFromJsonAsync<MappingDto>(ApiFactory.JsonOptions);

        var update = await _client.PutAsJsonAsync($"/api/applications/{fx.AppId}/mappings/{mapping!.Id}",
            new UpdateMappingRequest("name", "name", null, TransformationType.Uppercase, null), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<MappingDto>(ApiFactory.JsonOptions);
        Assert.Equal(TransformationType.Uppercase, updated!.Transformation);

        var delete = await _client.DeleteAsync($"/api/applications/{fx.AppId}/mappings/{mapping.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }
}
