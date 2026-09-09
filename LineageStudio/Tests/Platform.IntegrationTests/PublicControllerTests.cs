using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;
using Platform.Runtime.Execution;

namespace Platform.IntegrationTests;

/// <summary>
/// PublicController is the end-user-facing surface for a published app - no builder login. These
/// tests drive it with a plain, unauthenticated HttpClient (never CreateAuthenticatedClient) to
/// prove a real visitor, not just the builder, can reach it - and that it stays 404 until the
/// application is actually published.
/// </summary>
public class PublicControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _builderClient;

    public PublicControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _builderClient = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record CustomerAppFixture(Guid AppId, Guid ApiId, Guid ScreenId);

    private async Task<CustomerAppFixture> BuildCustomerAppAsync()
    {
        var appResponse = await _builderClient.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Customer Management", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var screenResponse = await _builderClient.PostAsJsonAsync($"/api/applications/{app!.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        var tableResponse = await _builderClient.PostAsJsonAsync($"/api/applications/{app.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true, IsNullable: false),
                new("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
                new("email", ColumnDataType.Varchar, Length: 200, IsUnique: true, IsNullable: false),
            }), ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var apiResponse = await _builderClient.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, null, table!.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        foreach (var fieldName in new[] { "name", "email" })
        {
            var componentResponse = await _builderClient.PostAsJsonAsync($"/api/applications/{app.Id}/components",
                new CreateComponentRequest(screen!.Id, ComponentType.Input, $"CustomerForm.{fieldName}", 0, 0),
                ApiFactory.JsonOptions);
            var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
            var column = table.Columns.Single(c => c.Name == fieldName);

            await _builderClient.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
                new CreateMappingRequest(component!.Id, fieldName, api!.Id, fieldName, null, null, column.Id, TransformationType.None, null),
                ApiFactory.JsonOptions);
        }

        return new CustomerAppFixture(app.Id, api!.Id, screen!.Id);
    }

    [Fact]
    public async Task An_unpublished_application_is_not_reachable_publicly()
    {
        var fx = await BuildCustomerAppAsync();

        var anonymous = _factory.CreateClient();
        var response = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_published_application_is_reachable_without_any_token_and_can_be_run()
    {
        var fx = await BuildCustomerAppAsync();
        await _builderClient.PostAsync($"/api/applications/{fx.AppId}/publish", null);

        var anonymous = _factory.CreateClient();

        var appResponse = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}");
        Assert.Equal(HttpStatusCode.OK, appResponse.StatusCode);

        var screensResponse = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}/screens");
        var screens = await screensResponse.Content.ReadFromJsonAsync<List<ScreenDto>>(ApiFactory.JsonOptions);
        Assert.Single(screens!);

        var componentsResponse = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}/screens/{fx.ScreenId}/components");
        var components = await componentsResponse.Content.ReadFromJsonAsync<List<ComponentDto>>(ApiFactory.JsonOptions);
        Assert.Equal(2, components!.Count);

        var mappingsResponse = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}/mappings");
        var mappings = await mappingsResponse.Content.ReadFromJsonAsync<List<MappingDto>>(ApiFactory.JsonOptions);
        Assert.Equal(2, mappings!.Count);

        var formData = new Dictionary<string, JsonElement>
        {
            ["name"] = JsonSerializer.SerializeToElement("Jane Doe"),
            ["email"] = JsonSerializer.SerializeToElement("jane@example.com"),
        };
        var executeResponse = await anonymous.PostAsJsonAsync(
            $"/api/public/apps/{fx.AppId}/execute", new RuntimeExecuteRequest(fx.ApiId, formData), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, executeResponse.StatusCode);

        var result = await executeResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.True(result!.Success);
        Assert.Single(result.Rows);
    }

    [Fact]
    public async Task Unpublishing_closes_off_public_access_again()
    {
        var fx = await BuildCustomerAppAsync();
        await _builderClient.PostAsync($"/api/applications/{fx.AppId}/publish", null);
        await _builderClient.PostAsync($"/api/applications/{fx.AppId}/unpublish", null);

        var anonymous = _factory.CreateClient();
        var response = await anonymous.GetAsync($"/api/public/apps/{fx.AppId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
