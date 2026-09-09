using FlowSphere.Application.Apps.Commands.DiscardApp;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class DiscardAppCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, AppDefinition App)> SeedAppAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, app);
    }

    [Fact]
    public async Task Handle_DevAppNeverPromoted_DeletesIt()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, app) = await SeedAppAsync(currentUser, nameof(Handle_DevAppNeverPromoted_DeletesIt));
        await using var _ = db;

        var handler = new DiscardAppCommandHandler(db, currentUser);
        var result = await handler.Handle(new DiscardAppCommand(app.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(await db.AppDefinitions.FindAsync(app.Id));
    }

    [Fact]
    public async Task Handle_AppAlreadyPromoted_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, app) = await SeedAppAsync(currentUser, nameof(Handle_AppAlreadyPromoted_ReturnsConflict));
        await using var _ = db;

        db.AppDefinitions.Add(app.CloneForPromotion(EnvironmentStage.QA));
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DiscardAppCommandHandler(db, currentUser);
        var result = await handler.Handle(new DiscardAppCommand(app.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(await db.AppDefinitions.FindAsync(app.Id));
    }

    [Fact]
    public async Task Handle_NonDevApp_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, app) = await SeedAppAsync(currentUser, nameof(Handle_NonDevApp_ReturnsConflict));
        await using var _ = db;

        var qaCopy = app.CloneForPromotion(EnvironmentStage.QA);
        db.AppDefinitions.Add(qaCopy);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DiscardAppCommandHandler(db, currentUser);
        var result = await handler.Handle(new DiscardAppCommand(qaCopy.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AppDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new DiscardAppCommandHandler(db, currentUser);
        var result = await handler.Handle(new DiscardAppCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
