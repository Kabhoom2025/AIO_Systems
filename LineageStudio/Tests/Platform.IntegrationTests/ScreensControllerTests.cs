using System.Net;
using System.Net.Http.Json;
using Platform.Application.Applications;
using Platform.Application.UiBuilder;

namespace Platform.IntegrationTests;

public class ScreensControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ScreensControllerTests(ApiFactory factory)
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

    [Fact]
    public async Task Create_screen_normalizes_the_route_and_roundtrips()
    {
        var appId = await CreateApplicationAsync("Screens App");

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/screens",
            new CreateScreenRequest("Customer Registration", "customer-registration"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var screen = await response.Content.ReadFromJsonAsync<ScreenDto>();
        Assert.Equal("/customer-registration", screen!.Route);
    }

    [Fact]
    public async Task Create_screen_with_duplicate_route_returns_conflict()
    {
        var appId = await CreateApplicationAsync("Duplicate Route App");
        await _client.PostAsJsonAsync($"/api/applications/{appId}/screens", new CreateScreenRequest("A", "/same"));

        var second = await _client.PostAsJsonAsync($"/api/applications/{appId}/screens", new CreateScreenRequest("B", "/same"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Update_and_delete_screen_work()
    {
        var appId = await CreateApplicationAsync("Update Screen App");
        var create = await _client.PostAsJsonAsync($"/api/applications/{appId}/screens", new CreateScreenRequest("Old Name", "/old"));
        var screen = await create.Content.ReadFromJsonAsync<ScreenDto>();

        var update = await _client.PutAsJsonAsync($"/api/applications/{appId}/screens/{screen!.Id}",
            new UpdateScreenRequest("New Name", "/new"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ScreenDto>();
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("/new", updated.Route);

        var delete = await _client.DeleteAsync($"/api/applications/{appId}/screens/{screen.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/applications/{appId}/screens/{screen.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
