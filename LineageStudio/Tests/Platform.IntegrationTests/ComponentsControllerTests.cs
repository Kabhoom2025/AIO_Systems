using System.Net;
using System.Net.Http.Json;
using Platform.Application.Applications;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

public class ComponentsControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ComponentsControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(Guid appId, Guid screenId)> CreateAppAndScreenAsync()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Components App", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var screenResponse = await _client.PostAsJsonAsync($"/api/applications/{app!.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        return (app.Id, screen!.Id);
    }

    [Fact]
    public async Task Create_component_roundtrips_with_json_fields()
    {
        var (appId, screenId) = await CreateAppAndScreenAsync();

        var request = new CreateComponentRequest(
            screenId, ComponentType.Input, "Name", 100, 200,
            PropertiesJson: """{"label":"Name","placeholder":"Enter your name"}""",
            ValidationJson: """{"required":true}""",
            DataBinding: "name");

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/components", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var component = await response.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
        Assert.Equal("name", component!.DataBinding);
        Assert.Contains("Enter your name", component.PropertiesJson);
    }

    [Fact]
    public async Task Create_component_rejects_malformed_json()
    {
        var (appId, screenId) = await CreateAppAndScreenAsync();

        var request = new CreateComponentRequest(screenId, ComponentType.Input, "Name", 0, 0, PropertiesJson: "{not valid json");
        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/components", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_components_can_be_filtered_by_screen()
    {
        var (appId, screenId) = await CreateAppAndScreenAsync();
        await _client.PostAsJsonAsync($"/api/applications/{appId}/components",
            new CreateComponentRequest(screenId, ComponentType.Label, "Header", 0, 0), ApiFactory.JsonOptions);

        var all = await _client.GetFromJsonAsync<List<ComponentDto>>($"/api/applications/{appId}/components", ApiFactory.JsonOptions);
        Assert.Single(all!);

        var filtered = await _client.GetFromJsonAsync<List<ComponentDto>>(
            $"/api/applications/{appId}/components?screenId={screenId}", ApiFactory.JsonOptions);
        Assert.Single(filtered!);
    }

    [Fact]
    public async Task Update_component_moves_its_position()
    {
        var (appId, screenId) = await CreateAppAndScreenAsync();
        var create = await _client.PostAsJsonAsync($"/api/applications/{appId}/components",
            new CreateComponentRequest(screenId, ComponentType.Button, "Save", 0, 0), ApiFactory.JsonOptions);
        var component = await create.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);

        var update = await _client.PutAsJsonAsync($"/api/applications/{appId}/components/{component!.Id}",
            new UpdateComponentRequest("Save", 50, 75), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
        Assert.Equal(50, updated!.PositionX);
        Assert.Equal(75, updated.PositionY);
    }

    [Fact]
    public async Task Delete_component_removes_it()
    {
        var (appId, screenId) = await CreateAppAndScreenAsync();
        var create = await _client.PostAsJsonAsync($"/api/applications/{appId}/components",
            new CreateComponentRequest(screenId, ComponentType.Checkbox, "Agree", 0, 0), ApiFactory.JsonOptions);
        var component = await create.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);

        var delete = await _client.DeleteAsync($"/api/applications/{appId}/components/{component!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/applications/{appId}/components/{component.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
