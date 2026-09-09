using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Platform.Application.Applications;
using Platform.Application.DataDesigner;
using Platform.Domain.Enums;

namespace Platform.IntegrationTests;

public class TablesControllerTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public TablesControllerTests(ApiFactory factory)
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
    public async Task Create_table_creates_a_real_postgres_table()
    {
        var appId = await CreateApplicationAsync("Customer Management");

        var request = new CreateTableRequest("customer", new List<ColumnDefinition>
        {
            new("id", ColumnDataType.Uuid, IsPrimaryKey: true, IsNullable: false),
            new("name", ColumnDataType.Varchar, Length: 200, IsNullable: false),
            new("email", ColumnDataType.Varchar, Length: 200, IsUnique: true, IsNullable: false),
            new("phone", ColumnDataType.Varchar, Length: 20, IsIndexed: true),
        });

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var table = await response.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);
        Assert.NotNull(table);
        Assert.Equal(4, table!.Columns.Count);
        Assert.Equal($"app_{appId:N}", table.SchemaName);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();

        await using var checkTable = connection.CreateCommand();
        checkTable.CommandText =
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name = @table";
        checkTable.Parameters.AddWithValue("schema", table.SchemaName);
        checkTable.Parameters.AddWithValue("table", "customer");
        var tableCount = (long)(await checkTable.ExecuteScalarAsync())!;
        Assert.Equal(1, tableCount);

        await using var insert = connection.CreateCommand();
        insert.CommandText =
            $"INSERT INTO \"{table.SchemaName}\".\"customer\" (id, name, email, phone) VALUES (gen_random_uuid(), @name, @email, @phone)";
        insert.Parameters.AddWithValue("name", "Jane Doe");
        insert.Parameters.AddWithValue("email", "jane@example.com");
        insert.Parameters.AddWithValue("phone", "5551234");
        var inserted = await insert.ExecuteNonQueryAsync();
        Assert.Equal(1, inserted);

        // Duplicate email must violate the real UNIQUE constraint we asked for.
        await using var duplicate = connection.CreateCommand();
        duplicate.CommandText =
            $"INSERT INTO \"{table.SchemaName}\".\"customer\" (id, name, email, phone) VALUES (gen_random_uuid(), @name, @email, @phone)";
        duplicate.Parameters.AddWithValue("name", "Jane Doe 2");
        duplicate.Parameters.AddWithValue("email", "jane@example.com");
        duplicate.Parameters.AddWithValue("phone", "5551234");
        await Assert.ThrowsAsync<PostgresException>(() => duplicate.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task Create_table_rejects_an_unsafe_identifier()
    {
        var appId = await CreateApplicationAsync("Injection Test App");

        var request = new CreateTableRequest("customer\"; DROP TABLE platform.applications; --", new List<ColumnDefinition>
        {
            new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
        });

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // The would-be-dropped table must still exist.
        var stillThere = await _client.GetAsync($"/api/applications/{appId}");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
    }

    [Fact]
    public async Task Create_table_rejects_a_mistyped_default_value()
    {
        var appId = await CreateApplicationAsync("Bad Default App");

        var request = new CreateTableRequest("widget", new List<ColumnDefinition>
        {
            new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
            new("quantity", ColumnDataType.Integer, DefaultValue: "not-a-number"),
        });

        var response = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables", request, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Foreign_key_references_a_real_column_in_another_table()
    {
        var appId = await CreateApplicationAsync("Orders App");

        var customerRequest = new CreateTableRequest("customer", new List<ColumnDefinition>
        {
            new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
        });
        var customerResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables", customerRequest, ApiFactory.JsonOptions);
        var customerTable = await customerResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);
        var customerIdColumn = customerTable!.Columns.Single(c => c.Name == "id");

        var orderRequest = new CreateTableRequest("order", new List<ColumnDefinition>
        {
            new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
            new("customer_id", ColumnDataType.Uuid, IsForeignKey: true,
                ReferencesTableId: customerTable.Id, ReferencesColumnId: customerIdColumn.Id),
        });
        var orderResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables", orderRequest, ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();
        await using var checkFk = connection.CreateCommand();
        checkFk.CommandText =
            "SELECT COUNT(*) FROM information_schema.table_constraints " +
            "WHERE table_schema = @schema AND table_name = 'order' AND constraint_type = 'FOREIGN KEY'";
        checkFk.Parameters.AddWithValue("schema", $"app_{appId:N}");
        var fkCount = (long)(await checkFk.ExecuteScalarAsync())!;
        Assert.Equal(1, fkCount);
    }

    [Fact]
    public async Task Add_and_delete_column_apply_real_ddl()
    {
        var appId = await CreateApplicationAsync("Column Ops App");
        var createResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables",
            new CreateTableRequest("product", new List<ColumnDefinition> { new("id", ColumnDataType.Uuid, IsPrimaryKey: true) }), ApiFactory.JsonOptions);
        var table = await createResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var addColumn = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables/{table!.Id}/columns",
            new AddColumnRequest(new ColumnDefinition("price", ColumnDataType.Decimal, Length: 10)), ApiFactory.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, addColumn.StatusCode);
        var column = await addColumn.Content.ReadFromJsonAsync<ColumnDto>(ApiFactory.JsonOptions);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();
        await using var checkColumn = connection.CreateCommand();
        checkColumn.CommandText =
            "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @schema AND table_name = 'product' AND column_name = 'price'";
        checkColumn.Parameters.AddWithValue("schema", table.SchemaName);
        Assert.Equal(1L, (long)(await checkColumn.ExecuteScalarAsync())!);

        var deleteColumn = await _client.DeleteAsync($"/api/applications/{appId}/tables/{table.Id}/columns/{column!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteColumn.StatusCode);

        Assert.Equal(0L, (long)(await checkColumn.ExecuteScalarAsync())!);
    }

    [Fact]
    public async Task Latest_rows_returns_real_inserted_data_most_recent_first()
    {
        var appId = await CreateApplicationAsync("Latest Rows App");
        var createResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables",
            new CreateTableRequest("customer", new List<ColumnDefinition>
            {
                new("id", ColumnDataType.Uuid, IsPrimaryKey: true),
                new("name", ColumnDataType.Varchar, Length: 200),
            }), ApiFactory.JsonOptions);
        var table = await createResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();

        foreach (var name in new[] { "Alice", "Bob", "Carol" })
        {
            await using var insert = connection.CreateCommand();
            insert.CommandText = $"INSERT INTO \"{table!.SchemaName}\".\"customer\" (id, name) VALUES (gen_random_uuid(), @name)";
            insert.Parameters.AddWithValue("name", name);
            await insert.ExecuteNonQueryAsync();
        }

        var response = await _client.GetAsync($"/api/applications/{appId}/tables/{table!.Id}/rows?limit=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rows = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>(ApiFactory.JsonOptions);
        Assert.Equal(2, rows!.Count);
        Assert.Equal("Carol", rows[0]["name"].ToString());
        Assert.Equal("Bob", rows[1]["name"].ToString());
    }

    [Fact]
    public async Task Delete_table_drops_the_real_postgres_table()
    {
        var appId = await CreateApplicationAsync("Drop Table App");
        var createResponse = await _client.PostAsJsonAsync($"/api/applications/{appId}/tables",
            new CreateTableRequest("temp", new List<ColumnDefinition> { new("id", ColumnDataType.Uuid, IsPrimaryKey: true) }), ApiFactory.JsonOptions);
        var table = await createResponse.Content.ReadFromJsonAsync<TableDto>(ApiFactory.JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/applications/{appId}/tables/{table!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await using var connection = new NpgsqlConnection(ApiFactory.TestConnectionString);
        await connection.OpenAsync();
        await using var checkTable = connection.CreateCommand();
        checkTable.CommandText =
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name = 'temp'";
        checkTable.Parameters.AddWithValue("schema", table.SchemaName);
        Assert.Equal(0L, (long)(await checkTable.ExecuteScalarAsync())!);
    }
}
