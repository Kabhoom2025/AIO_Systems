using FlowSphere.Application.Apps.Commands.MoveAppToWorkspace;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class MoveAppToWorkspaceCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Source, Workspace Target, AppDefinition DevApp, AppDefinition LiveClone, TableDefinition Table)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var source = Workspace.Create(currentUser.OrganizationId, "Source WS", null, currentUser.UserId);
        var target = Workspace.Create(currentUser.OrganizationId, "Target WS", null, currentUser.UserId);
        db.Workspaces.AddRange(source, target);
        await db.SaveChangesAsync(CancellationToken.None);

        var table = TableDefinition.Create(source.Id, "Requests", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync(CancellationToken.None);

        var devApp = AppDefinition.Create(source.Id, "Leave Request", null, currentUser.UserId);
        devApp.LinkTable(table.Id, "{}");
        db.AppDefinitions.Add(devApp);
        await db.SaveChangesAsync(CancellationToken.None);

        var liveClone = devApp.CloneForPromotion(EnvironmentStage.Live);
        db.AppDefinitions.Add(liveClone);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, source, target, devApp, liveClone, table);
    }

    [Fact]
    public async Task Handle_MovesAllStageClonesTogether()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, target, devApp, liveClone, _) = await SeedAsync(currentUser, nameof(Handle_MovesAllStageClonesTogether));
        await using var _db = db;

        var handler = new MoveAppToWorkspaceCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new MoveAppToWorkspaceCommand(devApp.Id, target.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var movedDev = await db.AppDefinitions.FindAsync(devApp.Id);
        var movedLive = await db.AppDefinitions.FindAsync(liveClone.Id);
        Assert.Equal(target.Id, movedDev!.WorkspaceId);
        Assert.Equal(target.Id, movedLive!.WorkspaceId);
    }

    [Fact]
    public async Task Handle_TableNotInTargetWorkspace_ClearsLinkedTableId()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, target, devApp, liveClone, _) = await SeedAsync(currentUser, nameof(Handle_TableNotInTargetWorkspace_ClearsLinkedTableId));
        await using var _db = db;

        var handler = new MoveAppToWorkspaceCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        await handler.Handle(new MoveAppToWorkspaceCommand(devApp.Id, target.Id), CancellationToken.None);

        var movedDev = await db.AppDefinitions.FindAsync(devApp.Id);
        var movedLive = await db.AppDefinitions.FindAsync(liveClone.Id);
        Assert.Null(movedDev!.LinkedTableId);
        Assert.Null(movedLive!.LinkedTableId);
    }

    [Fact]
    public async Task Handle_TableExistsInTargetWorkspace_KeepsLinkedTableId()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, target, devApp, _, table) = await SeedAsync(currentUser, nameof(Handle_TableExistsInTargetWorkspace_KeepsLinkedTableId));
        await using var _db = db;

        // Move the linked table itself into the target workspace first, so the app's link stays valid.
        table.WorkspaceId = target.Id;
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new MoveAppToWorkspaceCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        await handler.Handle(new MoveAppToWorkspaceCommand(devApp.Id, target.Id), CancellationToken.None);

        var movedDev = await db.AppDefinitions.FindAsync(devApp.Id);
        Assert.Equal(table.Id, movedDev!.LinkedTableId);
    }

    [Fact]
    public async Task Handle_OutsideDevStage_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, target, devApp, _, _) = await SeedAsync(currentUser, nameof(Handle_OutsideDevStage_ReturnsConflict));
        await using var _db = db;

        var handler = new MoveAppToWorkspaceCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext { Stage = EnvironmentStage.Live });
        var result = await handler.Handle(new MoveAppToWorkspaceCommand(devApp.Id, target.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AppBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };
        const string dbName = nameof(Handle_AppBelongsToAnotherOrganization_ReturnsNotFound);

        var (dbA, _, target, devAppA, _, _) = await SeedAsync(orgA, dbName);
        var targetId = target.Id;
        await dbA.DisposeAsync();

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new MoveAppToWorkspaceCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new MoveAppToWorkspaceCommand(devAppA.Id, targetId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_TargetWorkspaceInAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };
        const string dbName = nameof(Handle_TargetWorkspaceInAnotherOrganization_ReturnsNotFound);

        // Both orgs share the same underlying in-memory store (same dbName) but each DbContext
        // instance's global query filter only ever exposes its own org's rows - the established
        // cross-org test pattern in this suite (see SaveAppFormCommandHandlerTests).
        var db = TestDbContextFactory.Create(orgA, dbName);
        var sourceWorkspace = Workspace.Create(orgA.OrganizationId, "Org A Source", null, orgA.UserId);
        db.Workspaces.Add(sourceWorkspace);
        await db.SaveChangesAsync(CancellationToken.None);
        var devApp = AppDefinition.Create(sourceWorkspace.Id, "Leave Request", null, orgA.UserId);
        db.AppDefinitions.Add(devApp);
        await db.SaveChangesAsync(CancellationToken.None);
        var devAppId = devApp.Id;
        await db.DisposeAsync();

        await using var dbOrgB = TestDbContextFactory.Create(orgB, dbName);
        var otherOrgWorkspace = Workspace.Create(orgB.OrganizationId, "Org B WS", null, orgB.UserId);
        dbOrgB.Workspaces.Add(otherOrgWorkspace);
        await dbOrgB.SaveChangesAsync(CancellationToken.None);

        // Re-open as orgA so the app itself is visible, but the target workspace (org B's) isn't.
        await using var dbOrgA = TestDbContextFactory.Create(orgA, dbName);
        var handler = new MoveAppToWorkspaceCommandHandler(dbOrgA, orgA, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new MoveAppToWorkspaceCommand(devAppId, otherOrgWorkspace.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
