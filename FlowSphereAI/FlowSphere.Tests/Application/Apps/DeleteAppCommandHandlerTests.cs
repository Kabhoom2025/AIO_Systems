using FlowSphere.Application.Apps.Commands.DeleteApp;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class DeleteAppCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, int AppId)> SeedAppAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, app.Id);
    }

    [Fact]
    public async Task Handle_ExistingApp_DeletesAppAndItsRecords()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ExistingApp_DeletesAppAndItsRecords));
        await using var _ = db;

        db.AppRecords.Add(AppRecord.Create(appId, "{}", currentUser.UserId));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new DeleteAppCommand(appId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(await db.AppDefinitions.FindAsync(appId));
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_AppBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_AppBelongsToAnotherOrganization_ReturnsNotFound);
        var (dbA, appId) = await SeedAppAsync(orgA, dbName);
        await dbA.DisposeAsync();

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new DeleteAppCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new DeleteAppCommand(appId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AppDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new DeleteAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new DeleteAppCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
