using FlowSphere.Application.Tables.Commands.SaveTableSchema;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Tables;

public class SaveTableSchemaCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, int TableId)> SeedTableAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var table = TableDefinition.Create(workspace.Id, "Leads", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, table.Id);
    }

    [Fact]
    public async Task Handle_ValidSchema_SavesSchemaJson()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, tableId) = await SeedTableAsync(currentUser, nameof(Handle_ValidSchema_SavesSchemaJson));
        await using var _ = db;

        var handler = new SaveTableSchemaCommandHandler(db, currentUser);
        var schemaJson = """{"columns":[{"key":"name","type":"Text","label":"Name","required":true,"unique":false}]}""";

        var result = await handler.Handle(new SaveTableSchemaCommand(tableId, schemaJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.TableDefinitions.FindAsync(tableId);
        Assert.Equal(schemaJson, saved!.SchemaJson);
    }

    [Fact]
    public async Task Handle_SchemaMissingColumnsArray_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, tableId) = await SeedTableAsync(currentUser, nameof(Handle_SchemaMissingColumnsArray_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveTableSchemaCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveTableSchemaCommand(tableId, """{"notColumns":[]}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_TableBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_TableBelongsToAnotherOrganization_ReturnsNotFound);
        var (dbA, tableId) = await SeedTableAsync(orgA, dbName);
        await dbA.DisposeAsync();

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new SaveTableSchemaCommandHandler(dbB, orgB);

        var result = await handler.Handle(new SaveTableSchemaCommand(tableId, """{"columns":[]}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
