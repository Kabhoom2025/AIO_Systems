using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

public class ApplicationsControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApplicationsControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_then_get_roundtrips_the_application()
    {
        var create = await _client.PostAsJsonAsync("/api/applications",
            new CreateApplicationRequest("Customer Management", "Demo customer app"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ApplicationDto>();
        Assert.NotNull(created);
        Assert.Equal("Customer Management", created!.Name);
        Assert.False(created.IsPublished);

        var get = await _client.GetAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Create_with_duplicate_name_returns_conflict()
    {
        await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Duplicate App", null));

        var second = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Duplicate App", null));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_id_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/applications/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Publish_creates_a_version_and_marks_the_application_published()
    {
        var create = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Publishable App", null));
        var created = await create.Content.ReadFromJsonAsync<ApplicationDto>();

        var publish = await _client.PostAsync($"/api/applications/{created!.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var version = await publish.Content.ReadFromJsonAsync<ApplicationVersionDto>();
        Assert.Equal(1, version!.VersionNumber);

        var afterPublish = await (await _client.GetAsync($"/api/applications/{created.Id}")).Content.ReadFromJsonAsync<ApplicationDto>();
        Assert.True(afterPublish!.IsPublished);
        Assert.Equal(version.Id, afterPublish.CurrentVersionId);

        var versions = await _client.GetFromJsonAsync<List<ApplicationVersionDto>>($"/api/applications/{created.Id}/versions");
        Assert.Single(versions!);

        var unpublish = await _client.PostAsync($"/api/applications/{created.Id}/unpublish", null);
        var afterUnpublish = await unpublish.Content.ReadFromJsonAsync<ApplicationDto>();
        Assert.False(afterUnpublish!.IsPublished);
    }

    [Fact]
    public async Task Delete_removes_the_application()
    {
        var create = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("Deletable App", null));
        var created = await create.Content.ReadFromJsonAsync<ApplicationDto>();

        var delete = await _client.DeleteAsync($"/api/applications/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_also_drops_the_real_postgres_schema_it_created()
    {
        var create = await _client.PostAsJsonAsync("/api/applications", new CreateApplicationRequest("App With Real Data", null));
        var created = await create.Content.ReadFromJsonAsync<ApplicationDto>();

        var tableResponse = await _client.PostAsJsonAsync($"/api/applications/{created!.Id}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition> { new("id", ColumnDataType.Uuid, IsPrimaryKey: true) }),
            ApiFactory.JsonOptions);
        var table = await tableResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();
        await using var checkBefore = connection.CreateCommand();
        checkBefore.CommandText = "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = @schema";
        checkBefore.Parameters.AddWithValue("schema", table!.SchemaName);
        Assert.Equal(1L, (long)(await checkBefore.ExecuteScalarAsync())!);

        var delete = await _client.DeleteAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // Not just the metadata - the real schema (and every table in it) must be gone too, so
        // deleting an application never leaves orphaned real data behind in Postgres.
        await using var checkAfter = connection.CreateCommand();
        checkAfter.CommandText = "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = @schema";
        checkAfter.Parameters.AddWithValue("schema", table.SchemaName);
        Assert.Equal(0L, (long)(await checkAfter.ExecuteScalarAsync())!);
    }
}
