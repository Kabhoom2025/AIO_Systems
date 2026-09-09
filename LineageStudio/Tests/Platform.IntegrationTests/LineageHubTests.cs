using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;
using Platform.Lineage.Recording;
using Platform.Runtime.Execution;

namespace Platform.IntegrationTests;

/// <summary>
/// Proves the "no fake timers, real SignalR" requirement: a real HubConnection joins an
/// application's group, and a live "Save" through the runtime engine has to actually push
/// lineageEvent/lineageExecutionUpdate messages to it - nothing here is client-side simulated.
/// </summary>
public class LineageHubTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public LineageHubTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(Guid AppId, Guid ApiId)> BuildSimpleAppAsync()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Hub Test App", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var screenResponse = await _client.PostAsJsonAsync($"/api/applications/{app!.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        var tableResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
                new("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
            }), ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var componentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
            new CreateComponentRequest(screen!.Id, ComponentType.Input, "CustomerForm.name", 0, 0, DataBinding: "name"),
            ApiFactory.JsonOptions);
        var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);

        var apiResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, null, table!.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        var nameColumn = table.Columns.Single(c => c.Name == "name");
        await _client.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
            new CreateMappingRequest(component!.Id, "name", api!.Id, "name", null, null, nameColumn.Id, TransformationType.Trim, null),
            ApiFactory.JsonOptions);

        return (app.Id, api.Id);
    }

    [Fact]
    public async Task Saving_pushes_live_lineage_events_and_execution_updates_over_signalr()
    {
        var (appId, apiId) = await BuildSimpleAppAsync();

        await using var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/lineage", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(_factory.MintTestToken());
            })
            .AddJsonProtocol(opts => opts.PayloadSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))
            .Build();

        var receivedEvents = new List<LineageEventDto>();
        var receivedExecutionUpdates = new List<LineageExecutionDto>();
        connection.On<LineageEventDto>("lineageEvent", e => receivedEvents.Add(e));
        connection.On<LineageExecutionDto>("lineageExecutionUpdate", e => receivedExecutionUpdates.Add(e));

        await connection.StartAsync();
        await connection.InvokeAsync("JoinApplicationGroup", appId);

        var formData = new Dictionary<string, JsonElement> { ["name"] = JsonSerializer.SerializeToElement("Jane Doe") };
        var execResponse = await _client.PostAsJsonAsync($"/api/runtime/{appId}/execute",
            new RuntimeExecuteRequest(apiId, formData), ApiFactory.JsonOptions);
        var execResult = await execResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.True(execResult!.Success);

        // The hub push happens synchronously inside the HTTP request's execution, but SignalR
        // delivery to the client is a separate async hop - give it a moment to arrive.
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (receivedEvents.Count == 0 && DateTime.UtcNow < deadline)
            await Task.Delay(50);

        Assert.NotEmpty(receivedEvents);
        Assert.Contains(receivedEvents, e => e.EventType == LineageEventType.ExecutionCompleted);
        Assert.All(receivedEvents, e => Assert.Equal(appId, e.ApplicationId));

        Assert.NotEmpty(receivedExecutionUpdates);
        Assert.Contains(receivedExecutionUpdates, e => e.Status == ExecutionStatus.Success);

        await connection.StopAsync();
    }

    [Fact]
    public async Task A_client_that_never_joins_the_group_receives_nothing()
    {
        var (appId, apiId) = await BuildSimpleAppAsync();

        await using var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/lineage", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(_factory.MintTestToken());
            })
            .AddJsonProtocol(opts => opts.PayloadSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))
            .Build();

        var receivedEvents = new List<LineageEventDto>();
        connection.On<LineageEventDto>("lineageEvent", e => receivedEvents.Add(e));
        await connection.StartAsync();
        // Deliberately no JoinApplicationGroup call.

        var formData = new Dictionary<string, JsonElement> { ["name"] = JsonSerializer.SerializeToElement("John Smith") };
        await _client.PostAsJsonAsync($"/api/runtime/{appId}/execute", new RuntimeExecuteRequest(apiId, formData), ApiFactory.JsonOptions);

        await Task.Delay(300);
        Assert.Empty(receivedEvents);

        await connection.StopAsync();
    }
}
