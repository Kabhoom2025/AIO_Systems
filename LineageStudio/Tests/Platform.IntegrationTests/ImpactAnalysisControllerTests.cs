using System.Net.Http.Json;
using Platform.Application.ApiDesigner;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Application.ImpactAnalysis;
using Platform.Application.MappingDesigner;
using Platform.Application.UiBuilder;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

/// <summary>
/// Builds Customer(id, name) + Order(id, customer_id -> Customer.id) with a screen/API/service/
/// mapping wired to Customer.name, then asks the spec's own impact-analysis questions of it:
/// which screens/APIs/tables use a table or column, and what references what by foreign key.
/// </summary>
public class ImpactAnalysisControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ImpactAnalysisControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private record Fixture(Guid CustomerTableId, Guid CustomerIdColumnId, Guid CustomerNameColumnId, Guid OrderTableId, Guid CustomerFkColumnId);

    private async Task<Fixture> BuildCustomerOrderAppAsync()
    {
        var appResponse = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Impact App", null));
        var app = await appResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var customerResponse = await _client.PostAsJsonAsync($"/api/applications/{app!.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
                new("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
            }), ApiFactory.JsonOptions);
        var customerTable = await customerResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);
        var customerIdColumn = customerTable!.Columns.Single(c => c.Name == "id");
        var customerNameColumn = customerTable.Columns.Single(c => c.Name == "name");

        var orderResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/tables",
            new CreateTableRequest("order", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
                new("customer_id", ColumnDataType.Uuid, IsForeignKey: true,
                    ReferencesTableId: customerTable.Id, ReferencesColumnId: customerIdColumn.Id),
            }), ApiFactory.JsonOptions);
        var orderTable = await orderResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);
        var customerFkColumn = orderTable!.Columns.Single(c => c.Name == "customer_id");

        var screenResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/screens",
            new CreateScreenRequest("Customer Registration", "/customer-registration"));
        var screen = await screenResponse.Content.ReadFromJsonAsync<ScreenDto>();

        var componentResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/components",
            new CreateComponentRequest(screen!.Id, ComponentType.Input, "CustomerForm.name", 0, 0, DataBinding: "name"),
            ApiFactory.JsonOptions);
        var component = await componentResponse.Content.ReadFromJsonAsync<ComponentDto>(ApiFactory.JsonOptions);

        var serviceResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/services",
            new CreateServiceRequest("CustomerService", customerTable.Id, null));
        var service = await serviceResponse.Content.ReadFromJsonAsync<ServiceDto>();

        var apiResponse = await _client.PostAsJsonAsync($"/api/applications/{app.Id}/apis",
            new CreateApiEndpointRequest(ApiHttpMethod.Post, "/customer", null, null, service!.Id, customerTable.Id), ApiFactory.JsonOptions);
        var api = await apiResponse.Content.ReadFromJsonAsync<ApiEndpointDto>(ApiFactory.JsonOptions);

        await _client.PostAsJsonAsync($"/api/applications/{app.Id}/mappings",
            new CreateMappingRequest(component!.Id, "name", api!.Id, "name", service.Id, "name", customerNameColumn.Id, TransformationType.Trim, null),
            ApiFactory.JsonOptions);

        return new Fixture(customerTable.Id, customerIdColumn.Id, customerNameColumn.Id, orderTable.Id, customerFkColumn.Id);
    }

    [Fact]
    public async Task Table_impact_answers_which_screens_apis_and_tables_use_it()
    {
        var fx = await BuildCustomerOrderAppAsync();

        var impact = await _client.GetFromJsonAsync<TableImpactDto>($"/api/impact/table/{fx.CustomerTableId}");

        Assert.Single(impact!.Screens);
        Assert.Equal("Customer Registration", impact.Screens[0].Name);

        Assert.Single(impact.Apis);
        Assert.Equal("/customer", impact.Apis[0].Path);

        Assert.Single(impact.Services);
        Assert.Equal("CustomerService", impact.Services[0].Name);

        // order.customer_id has an FK pointing at customer - so order is a "referencing table".
        Assert.Single(impact.ReferencingTables);
        Assert.Equal("order", impact.ReferencingTables[0].Name);

        // customer itself has no outgoing FKs.
        Assert.Empty(impact.ReferencedTables);
    }

    [Fact]
    public async Task Order_table_impact_shows_it_references_customer()
    {
        var fx = await BuildCustomerOrderAppAsync();

        var impact = await _client.GetFromJsonAsync<TableImpactDto>($"/api/impact/table/{fx.OrderTableId}");

        Assert.Single(impact!.ReferencedTables);
        Assert.Equal("customer", impact.ReferencedTables[0].Name);
        Assert.Empty(impact.ReferencingTables);
        // No screen/API/service touches order directly in this fixture.
        Assert.Empty(impact.Screens);
    }

    [Fact]
    public async Task Column_impact_answers_which_ui_fields_and_api_fields_use_it()
    {
        var fx = await BuildCustomerOrderAppAsync();

        var impact = await _client.GetFromJsonAsync<ColumnImpactDto>($"/api/impact/column/{fx.CustomerNameColumnId}");

        Assert.Single(impact!.Components);
        Assert.Equal("CustomerForm.name", impact.Components[0].Name);

        Assert.Single(impact.ApiFields);
        Assert.Equal("name", impact.ApiFields[0].ApiField);
        Assert.Equal("/customer", impact.ApiFields[0].Path);

        Assert.Single(impact.ServiceFields);
        Assert.Equal("CustomerService", impact.ServiceFields[0].ServiceName);
    }

    [Fact]
    public async Task Column_impact_shows_what_would_break_if_a_referenced_pk_column_changes()
    {
        var fx = await BuildCustomerOrderAppAsync();

        var impact = await _client.GetFromJsonAsync<ColumnImpactDto>($"/api/impact/column/{fx.CustomerIdColumnId}");

        Assert.Single(impact!.DependentColumns);
        Assert.Equal("customer_id", impact.DependentColumns[0].Name);
        Assert.Equal("order", impact.DependentColumns[0].TableName);
    }

    [Fact]
    public async Task Unmapped_column_has_no_impact()
    {
        var fx = await BuildCustomerOrderAppAsync();

        var impact = await _client.GetFromJsonAsync<ColumnImpactDto>($"/api/impact/column/{fx.CustomerFkColumnId}");

        Assert.Empty(impact!.Components);
        Assert.Empty(impact.ApiFields);
        Assert.Empty(impact.ServiceFields);
        Assert.Empty(impact.DependentColumns);
    }
}
