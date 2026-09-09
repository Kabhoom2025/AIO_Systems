using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.DTOs;
using NovaERP.Infrastructure.Data;
using Xunit;

namespace NovaERP.Tests.Integration;

/// <summary>Exercises the Dashboard Builder's self-service scoping — a Dashboard belongs to a
/// User, not a Role, so every action here is reachable with plain [Authorize] and no
/// permission policy; ownership is enforced in the service layer instead (403, not 404, for a
/// dashboard that exists but isn't the caller's).</summary>
public class DashboardEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DashboardEndpointTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    private async Task EnsureSeededAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaErpDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedData.SeedAsync(db);
    }

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return body!.Token;
    }

    [Fact]
    public async Task Get_MyDashboards_Returns_Seeded_Dashboard_With_Three_Widgets()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync("/api/my-dashboards");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboards = await response.Content.ReadFromJsonAsync<List<DashboardDto>>();
        var dashboard = Assert.Single(dashboards!);
        Assert.Equal("My Dashboard", dashboard.Name);
        Assert.Equal(3, dashboard.Widgets.Count);
    }

    [Fact]
    public async Task Another_Users_Dashboard_Is_403_Not_404()
    {
        await EnsureSeededAsync();

        var employeeClient = _factory.CreateClient();
        employeeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(employeeClient, "employee@novaerp.local", "Employee@123"));
        var dashboardsResponse = await employeeClient.GetAsync("/api/my-dashboards");
        var dashboards = await dashboardsResponse.Content.ReadFromJsonAsync<List<DashboardDto>>();
        var employeeDashboardId = dashboards!.Single().Id;

        var managerClient = _factory.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(managerClient, "manager@novaerp.local", "Manager@123"));

        var response = await managerClient.GetAsync($"/api/my-dashboards/{employeeDashboardId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AddWidget_Reorder_Resize_Remove_Full_Lifecycle()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "manager@novaerp.local", "Manager@123"));

        var createResponse = await client.PostAsJsonAsync("/api/my-dashboards", new CreateDashboardDto { Name = "Manager Dashboard" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardDto>();

        var addResponse = await client.PostAsJsonAsync($"/api/my-dashboards/{created!.Id}/widgets",
            new AddWidgetDto { WidgetType = "PosSalesTotal", Title = "POS Sales", SizeOption = "Small" });
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);
        var afterAdd = await addResponse.Content.ReadFromJsonAsync<DashboardDto>();
        var widget = Assert.Single(afterAdd!.Widgets);
        Assert.Equal("Small", widget.SizeOption);

        var addResponse2 = await client.PostAsJsonAsync($"/api/my-dashboards/{created.Id}/widgets",
            new AddWidgetDto { WidgetType = "SalesOrderStatusSummary", Title = "Sales Orders", SizeOption = "Medium" });
        var afterAdd2 = await addResponse2.Content.ReadFromJsonAsync<DashboardDto>();
        Assert.Equal(2, afterAdd2!.Widgets.Count);

        var layoutResponse = await client.PutAsJsonAsync($"/api/my-dashboards/{created.Id}/widgets/layout", new ReorderWidgetsDto
        {
            Widgets = afterAdd2.Widgets.Select(w => new WidgetLayoutEntryDto
            {
                WidgetId = w.Id,
                DisplayOrder = w.WidgetType == "SalesOrderStatusSummary" ? 1 : 2,
                SizeOption = w.WidgetType == "PosSalesTotal" ? "Large" : w.SizeOption
            }).ToList()
        });
        Assert.Equal(HttpStatusCode.OK, layoutResponse.StatusCode);
        var afterLayout = await layoutResponse.Content.ReadFromJsonAsync<DashboardDto>();
        Assert.Equal("SalesOrderStatusSummary", afterLayout!.Widgets[0].WidgetType);
        Assert.Equal("Large", afterLayout.Widgets.First(w => w.WidgetType == "PosSalesTotal").SizeOption);

        var widgetToRemove = afterLayout.Widgets.First(w => w.WidgetType == "PosSalesTotal");
        var removeResponse = await client.DeleteAsync($"/api/my-dashboards/{created.Id}/widgets/{widgetToRemove.Id}");
        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
        var afterRemove = await removeResponse.Content.ReadFromJsonAsync<DashboardDto>();
        Assert.Single(afterRemove!.Widgets);
    }

    [Theory]
    [InlineData("SalesOrderStatusSummary")]
    [InlineData("ServiceTicketStatusSummary")]
    [InlineData("PosSalesTotal")]
    [InlineData("ProjectTaskStatusSummary")]
    public async Task GetWidgetData_Returns_Expected_Shape_For_Each_WidgetType(string widgetType)
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            await LoginAsync(client, "employee@novaerp.local", "Employee@123"));

        var response = await client.GetAsync($"/api/my-dashboards/widget-data/{widgetType}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<WidgetDataDto>();
        Assert.NotNull(data);
        Assert.Equal(data!.Labels.Count, data.Values.Count);
        Assert.NotEmpty(data.Labels);
    }

    [Fact]
    public async Task Get_MyDashboards_Requires_Authentication()
    {
        await EnsureSeededAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/my-dashboards");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
