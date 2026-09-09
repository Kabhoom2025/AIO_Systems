using FlowSphere.Application.Tables.Commands.DeleteTable;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Tables;

public class DeleteTableCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, int WorkspaceId, int TableId)> SeedTableAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var table = TableDefinition.Create(workspace.Id, "Leads", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, workspace.Id, table.Id);
    }

    [Fact]
    public async Task Handle_ExistingTable_DeletesTableAndItsRecords()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, tableId) = await SeedTableAsync(currentUser, nameof(Handle_ExistingTable_DeletesTableAndItsRecords));
        await using var _disposable = db;

        db.TableRecords.Add(TableRecord.Create(tableId, "{}", currentUser.UserId));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteTableCommandHandler(db, currentUser);
        var result = await handler.Handle(new DeleteTableCommand(tableId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(await db.TableDefinitions.FindAsync(tableId));
        Assert.Empty(db.TableRecords);
    }

    [Fact]
    public async Task Handle_TableLinkedFromAnApp_UnlinksTheApp()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspaceId, tableId) = await SeedTableAsync(currentUser, nameof(Handle_TableLinkedFromAnApp_UnlinksTheApp));
        await using var _disposable = db;

        var app = AppDefinition.Create(workspaceId, "Lead Intake", null, currentUser.UserId);
        app.LinkTable(tableId, """{"name":"leadName"}""");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteTableCommandHandler(db, currentUser);
        var result = await handler.Handle(new DeleteTableCommand(tableId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloadedApp = await db.AppDefinitions.FindAsync(app.Id);
        Assert.Null(reloadedApp!.LinkedTableId);
    }

    [Fact]
    public async Task Handle_TableBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_TableBelongsToAnotherOrganization_ReturnsNotFound);
        var (dbA, _, tableId) = await SeedTableAsync(orgA, dbName);
        await dbA.DisposeAsync();

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new DeleteTableCommandHandler(dbB, orgB);

        var result = await handler.Handle(new DeleteTableCommand(tableId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
