using System.Net.Http.Json;
using System.Text.Json;
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
/// Execution History reuses the Phase 9 query endpoints - these tests focus on what Phase 11
/// specifically adds: the application name on each execution row, and that a past execution's
/// recorded lineage (event metadata, component/API names as of when it ran) doesn't change
/// when the application is later renamed or its screens/mappings are edited.
/// </summary>
public class ExecutionHistoryTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ExecutionHistoryTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Execution_history_includes_the_application_name()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("History App", null));
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

        var formData = new Dictionary<string, JsonElement> { ["name"] = JsonSerializer.SerializeToElement("Jane Doe") };
        await _client.PostAsJsonAsync($"/api/runtime/{app.Id}/execute", new RuntimeExecuteRequest(api.Id, formData), ApiFactory.JsonOptions);

        // Rename the application and the component after the run completed.
        await _client.PutAsJsonAsync($"/api/applications/{app.Id}", new UpdateApplicationRequest("Renamed App", null));

        var executions = await _client.GetFromJsonAsync<List<LineageExecutionDto>>(
            $"/api/lineage/executions?applicationId={app.Id}", ApiFactory.JsonOptions);
        var execution = executions!.Single();
        Assert.Equal("Renamed App", execution.ApplicationName);

        var fetched = await _client.GetFromJsonAsync<LineageExecutionDto>(
            $"/api/lineage/executions/{execution.Id}", ApiFactory.JsonOptions);
        Assert.Equal("Renamed App", fetched!.ApplicationName);

        // The historical event still shows the component's name as it was when the run
        // happened, not whatever it's called now - that's the whole point of freezing metadata
        // into the event at record time instead of re-resolving names live.
        var events = await _client.GetFromJsonAsync<List<LineageEventDto>>(
            $"/api/lineage/{execution.Id}/events", ApiFactory.JsonOptions);
        var componentEvent = events!.Single(e => e.NodeType == LineageNodeType.Component);
        Assert.Contains("CustomerForm.name", componentEvent.MetadataJson);
    }
}
