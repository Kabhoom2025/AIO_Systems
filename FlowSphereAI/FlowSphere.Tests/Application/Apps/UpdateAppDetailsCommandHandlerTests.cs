using FlowSphere.Application.Apps.Commands.UpdateAppDetails;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class UpdateAppDetailsCommandHandlerTests
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
    public async Task Handle_ValidDetails_UpdatesNameDescriptionAndIcon()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ValidDetails_UpdatesNameDescriptionAndIcon));
        await using var _ = db;

        var handler = new UpdateAppDetailsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new UpdateAppDetailsCommand(appId, "Renamed App", "New desc", "Briefcase"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(appId);
        Assert.Equal("Renamed App", saved!.Name);
        Assert.Equal("New desc", saved.Description);
        Assert.Equal("Briefcase", saved.Icon);
    }

    [Fact]
    public async Task Handle_OutsideDevStage_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_OutsideDevStage_ReturnsConflict));
        await using var _ = db;

        var handler = new UpdateAppDetailsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext { Stage = FlowSphere.Domain.Enums.EnvironmentStage.Live });
        var result = await handler.Handle(new UpdateAppDetailsCommand(appId, "Renamed App", null, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
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
        var handler = new UpdateAppDetailsCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new UpdateAppDetailsCommand(appId, "Sneaky Rename", null, null), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
