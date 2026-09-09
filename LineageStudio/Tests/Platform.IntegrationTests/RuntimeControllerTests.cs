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

public class RuntimeControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public RuntimeControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record CustomerAppFixture(Guid AppId, Guid ApiId, Guid TableId, string SchemaName);

    /// <summary>Builds the exact Customer Registration demo from the spec: customer(id, name,
    /// email, phone), POST /api/customer, with CustomerForm.name/email/phone -> customer
    /// columns, email trimmed+lowercased and backed by a real UNIQUE constraint.</summary>
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
                new("phone", ColumnDataType.Varchar, Length: 20),
            }), ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var serviceResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/services",
            new CreateServiceRequest("CustomerService", table!.Id, null));
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var apiResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, service!.Id, table.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        var fieldNames = new[] { "name", "email", "phone" };
        foreach (var fieldName in fieldNames)
        {
            var componentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
                new CreateComponentRequest(screen!.Id, ComponentType.Input, $"CustomerForm.{fieldName}", 0, 0, DataBinding: fieldName),
                ApiFactory.JsonOptions);
            var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);
            var column = table.Columns.Single(c => c.Name == fieldName);

            var transformation = fieldName == "email" ? TransformationType.Lowercase : TransformationType.Trim;
            await _client.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
                new CreateMappingRequest(component!.Id, fieldName, api!.Id, fieldName, service.Id, fieldName, column.Id, transformation, null),
                ApiFactory.JsonOptions);
        }

        return new CustomerAppFixture(app.Id, api!.Id, table.Id, table.SchemaName);
    }

    private static Dictionary<string, JsonElement> FormData(params (string Key, string Value)[] fields)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (key, value) in fields)
            result[key] = JsonSerializer.SerializeToElement(value);
        return result;
    }

    [Fact]
    public async Task Execute_post_inserts_a_real_row_and_applies_transformations()
    {
        var fx = await BuildCustomerAppAsync();

        var request = new RuntimeExecuteRequest(fx.ApiId, FormData(
            ("name", "  Jane Doe  "), ("email", "JANE@EXAMPLE.COM"), ("phone", "555-1234")));

        var response = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.True(result!.Success);
        Assert.Single(result.Rows);

        var row = result.Rows[0];
        Assert.Equal("Jane Doe", ((JsonElement)row["name"]!).GetString());
        Assert.Equal("jane@example.com", ((JsonElement)row["email"]!).GetString());
    }

    [Fact]
    public async Task Execute_post_with_duplicate_email_fails_with_a_sanitized_error()
    {
        var fx = await BuildCustomerAppAsync();

        var first = new RuntimeExecuteRequest(fx.ApiId, FormData(
            ("name", "Jane Doe"), ("email", "dup@example.com"), ("phone", "5551111")));
        var firstResponse = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute", first, ApiFactory.JsonOptions);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.True(firstResult!.Success);

        var second = new RuntimeExecuteRequest(fx.ApiId, FormData(
            ("name", "John Smith"), ("email", "dup@example.com"), ("phone", "5552222")));
        var secondResponse = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute", second, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var secondResult = await secondResponse.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.False(secondResult!.Success);
        Assert.Equal("DUPLICATE_KEY", secondResult.ErrorCode);
        Assert.DoesNotContain("at Npgsql", secondResult.ErrorMessage);
        Assert.DoesNotContain("StackTrace", secondResult.ErrorMessage ?? "");
    }

    [Fact]
    public async Task Execute_post_with_missing_required_field_fails_validation()
    {
        var fx = await BuildCustomerAppAsync();

        // Fields genuinely missing from the submission (not merely empty strings, which Postgres
        // treats as satisfying NOT NULL) - e.g. a required screen field the user never filled in.
        var request = new RuntimeExecuteRequest(fx.ApiId, []);
        var response = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);
        Assert.False(result!.Success);
        Assert.Equal("VALIDATION_FAILED", result.ErrorCode);
    }

    [Fact]
    public async Task Execute_get_returns_previously_inserted_rows()
    {
        var fx = await BuildCustomerAppAsync();
        await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(fx.ApiId, FormData(("name", "Jane Doe"), ("email", "jane2@example.com"), ("phone", "555"))),
            ApiFactory.JsonOptions);

        var getApiResponse = await _client.PostAsJsonAsync($"/api/applications/{fx.AppId}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Get, "/customer", null, null, null, fx.TableId), ApiFactory.JsonOptions);
        var getApi = await getApiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        var response = await _client.PostAsJsonAsync($"/api/runtime/{fx.AppId}/execute",
            new RuntimeExecuteRequest(getApi!.Id, []), ApiFactory.JsonOptions);
        var result = await response.Content.ReadFromJsonAsync<RuntimeExecutionResult>(ApiFactory.JsonOptions);

        Assert.True(result!.Success);
        Assert.Single(result.Rows);
    }
}
