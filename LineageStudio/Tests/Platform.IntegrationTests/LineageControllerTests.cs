using System.Net;
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

public class LineageControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public LineageControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record CustomerAppFixture(Guid AppId, Guid ApiId);

    private async Task<CustomerAppFixture> BuildCustomerAppAsync()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Customer Management", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var screenResponse = await _client.PostAsJsonAsync($"/api/applications/{app!.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        var tableResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true, IsNullable: false),
                new("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
                new("email", ColumnDataType.Varchar, Length: 200, IsUnique: true, IsNullable: false),
                new("age", ColumnDataType.Integer, IsNullable: true),
            }), ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var serviceResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/services",
            new CreateServiceRequest("CustomerService", table!.Id, null));
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var apiResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, service!.Id, table.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        foreach (var fieldName in new[] { "name", "email" })
        {
            var componentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
                new CreateComponentRequest(screen!.Id, ComponentType.Input, $"CustomerForm.{fieldName}", 0, 0, DataBinding: fieldName),
                ApiFactory.JsonOptions);
            var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
            var column = table.Columns.Single(c => c.Name == fieldName);

            await _client.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
                new CreateMappingRequest(component!.Id, fieldName, api!.Id, fieldName, service.Id, fieldName, column.Id, TransformationType.Trim, null),
                ApiFactory.JsonOptions);
        }

        // Nullable "age" mapped through NumberConversion - left unsubmitted in most tests (parses
        // to null, which is fine since it's nullable), but submitting a non-numeric value for it
        // is how the "failed during transformation" scenario is exercised below.
        var ageComponentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
            new CreateComponentRequest(screen!.Id, ComponentType.Number, "CustomerForm.age", 0, 0, DataBinding: "age"),
            ApiFactory.JsonOptions);
        var ageComponent = await ageComponentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
        var ageColumn = table.Columns.Single(c => c.Name == "age");
        await _client.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
            new CreateMappingRequest(ageComponent!.Id, "age", api!.Id, "age", service.Id, "age", ageColumn.Id, TransformationType.NumberConversion, "{}"),
            ApiFactory.JsonOptions);

        return new CustomerAppFixture(app.Id, api!.Id);
    }

    private static Dictionary<string, JsonElement> FormData(params (string Key, string Value)[] fields)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (key, value) in fields)
            result[key] = JsonSerializer.SerializeToElement(value);
        return result;
    }

    [Fact]
    public async Task Successful_execution_records_a_full_success_event_chain()
    {
        var fx = await BuildCustomerAppAsync();

        var execResponse = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(fx.ApiId, FormData(("name", "Jane Doe"), ("email", "jane@example.com"))),
            ApiFactory.JsonOptions);
        var execResult = await execResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.True(execResult!.Success);

        var executions = await _client.GetFromJsonAsync<List<LineageExecutionDto>>(
            $"/api/lineage/executions?applicationId={fx.AppId}", ApiFactory.JsonOptions);
        Assert.Single(executions!);
        var execution = executions![0];
        Assert.Equal(ExecutionStatus.Success, execution.Status);
        Assert.NotNull(execution.CompletedAt);
        Assert.NotNull(execution.DurationMs);
        Assert.NotEqual(Guid.Empty, execution.VersionId);

        var fetched = await _client.GetFromJsonAsync<LineageExecutionDto>(
            $"/api/lineage/executions/{execution.Id}", ApiFactory.JsonOptions);
        Assert.Equal(execution.Id, fetched!.Id);

        var events = await _client.GetFromJsonAsync<List<LineageEventDto>>(
            $"/api/lineage/{execution.Id}/events", ApiFactory.JsonOptions);

        var eventTypes = events!.Select(e => e.EventType).ToList();
        Assert.Contains(LineageEventType.ScreenStarted, eventTypes);
        Assert.Contains(LineageEventType.ComponentStarted, eventTypes);
        Assert.Contains(LineageEventType.ApiStarted, eventTypes);
        Assert.Contains(LineageEventType.TransformationStarted, eventTypes);
        Assert.Contains(LineageEventType.TransformationCompleted, eventTypes);
        Assert.Contains(LineageEventType.ServiceStarted, eventTypes);
        Assert.Contains(LineageEventType.ServiceCompleted, eventTypes);
        Assert.Contains(LineageEventType.DatabaseStarted, eventTypes);
        Assert.Contains(LineageEventType.DatabaseCompleted, eventTypes);
        Assert.Contains(LineageEventType.ApiCompleted, eventTypes);
        Assert.Contains(LineageEventType.ExecutionCompleted, eventTypes);
        Assert.DoesNotContain(LineageEventType.DatabaseFailed, eventTypes);

        // Every event traces back to the same execution and none carry a raw stack trace.
        Assert.All(events!, e => Assert.Equal(execution.Id, e.ExecutionId));
        Assert.All(events!, e => Assert.DoesNotContain("at Platform.", e.ErrorMessage ?? ""));
    }

    [Fact]
    public async Task Failed_execution_records_database_failed_and_skips_nothing_upstream()
    {
        var fx = await BuildCustomerAppAsync();

        await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(fx.ApiId, FormData(("name", "Jane Doe"), ("email", "dup@example.com"))),
            ApiFactory.JsonOptions);

        var secondResponse = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(fx.ApiId, FormData(("name", "John Smith"), ("email", "dup@example.com"))),
            ApiFactory.JsonOptions);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.False(secondResult!.Success);

        var executions = await _client.GetFromJsonAsync<List<LineageExecutionDto>>(
            $"/api/lineage/executions?applicationId={fx.AppId}", ApiFactory.JsonOptions);
        var failedExecution = executions!.Single(e => e.Status == ExecutionStatus.Failed);

        var events = await _client.GetFromJsonAsync<List<LineageEventDto>>(
            $"/api/lineage/{failedExecution.Id}/events", ApiFactory.JsonOptions);

        var databaseFailed = events!.Single(e => e.EventType == LineageEventType.DatabaseFailed);
        Assert.Equal("DUPLICATE_KEY", databaseFailed.ErrorCode);
        Assert.NotNull(databaseFailed.ErrorMessage);

        // Both transformations succeeded before the real INSERT was attempted and failed -
        // nothing upstream of the database should be marked skipped in this scenario.
        Assert.DoesNotContain(events!, e => e.Status == ExecutionStatus.Skipped);
        Assert.Contains(events!, e => e.EventType == LineageEventType.ApiFailed);
        Assert.Contains(events!, e => e.EventType == LineageEventType.ExecutionCompleted && e.Status == ExecutionStatus.Failed);
    }

    [Fact]
    public async Task Validation_failure_skips_downstream_service_and_database_nodes()
    {
        var fx = await BuildCustomerAppAsync();

        // A non-numeric "age" fails NumberConversion during the transformation step itself -
        // before any DML runs - unlike a missing required field, which only fails later at
        // insert time (see Failed_execution_records_database_failed... for that path instead).
        var response = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(fx.ApiId, FormData(("name", "Jane Doe"), ("email", "jane3@example.com"), ("age", "not-a-number"))),
            ApiFactory.JsonOptions);
        var result = await response.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.False(result!.Success);
        Assert.Equal("VALIDATION_FAILED", result.ErrorCode);

        var executions = await _client.GetFromJsonAsync<List<LineageExecutionDto>>(
            $"/api/lineage/executions?applicationId={fx.AppId}", ApiFactory.JsonOptions);
        var execution = executions!.Single();

        var events = await _client.GetFromJsonAsync<List<LineageEventDto>>(
            $"/api/lineage/{execution.Id}/events", ApiFactory.JsonOptions);

        Assert.Contains(events!, e => e.EventType == LineageEventType.ValidationFailed);

        var serviceEvent = events!.Single(e => e.NodeType == LineageNodeType.Service);
        Assert.Equal(ExecutionStatus.Skipped, serviceEvent.Status);

        var databaseEvent = events!.Single(e => e.NodeType == LineageNodeType.Database);
        Assert.Equal(ExecutionStatus.Skipped, databaseEvent.Status);

        Assert.Equal(ExecutionStatus.Failed, execution.Status);
    }
}
