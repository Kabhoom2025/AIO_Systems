using FlowSphere.Application.Workspaces.Commands.CopyWorkspace;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Workspaces;

public class CopyWorkspaceCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Source, AppDefinition App, TableDefinition Table)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var source = Workspace.Create(currentUser.OrganizationId, "Source Workspace", null, currentUser.UserId);
        db.Workspaces.Add(source);
        await db.SaveChangesAsync();

        var table = TableDefinition.Create(source.Id, "Employees", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(source.Id, "Leave Request", null, currentUser.UserId);
        app.UpdateFormSchema("""{"sections":[{"id":"s1","title":"S1","fields":[]}]}""");
        app.LinkTable(table.Id, "{}");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, source, app, table);
    }

    [Fact]
    public async Task Handle_CopyBoth_ClonesAppsAndTablesWithFreshLineage()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, source, app, table) = await SeedAsync(currentUser, nameof(Handle_CopyBoth_ClonesAppsAndTablesWithFreshLineage));
        await using var _ = db;

        var handler = new CopyWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new CopyWorkspaceCommand(source.Id, "Copied Workspace", CopyApps: true, CopyTables: true, AdminUserId: null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var newWorkspaceId = result.Value;

        var copiedApp = db.AppDefinitions.Single(a => a.WorkspaceId == newWorkspaceId);
        Assert.Equal(app.Name, copiedApp.Name);
        Assert.Equal(app.FormSchemaJson, copiedApp.FormSchemaJson);
        Assert.NotEqual(app.SourceGroupId, copiedApp.SourceGroupId);
        Assert.Equal(EnvironmentStage.Dev, copiedApp.Stage);
        Assert.Null(copiedApp.LinkedTableId);

        var copiedTable = db.TableDefinitions.Single(t => t.WorkspaceId == newWorkspaceId);
        Assert.Equal(table.Name, copiedTable.Name);
    }

    [Fact]
    public async Task Handle_CopyAppsOnly_DoesNotCopyTables()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, source, _, _) = await SeedAsync(currentUser, nameof(Handle_CopyAppsOnly_DoesNotCopyTables));
        await using var _ = db;

        var handler = new CopyWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new CopyWorkspaceCommand(source.Id, "Copied Workspace", CopyApps: true, CopyTables: false, AdminUserId: null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.AppDefinitions.Where(a => a.WorkspaceId == result.Value));
        Assert.Empty(db.TableDefinitions.Where(t => t.WorkspaceId == result.Value));
    }

    [Fact]
    public async Task Handle_WithAdminUserId_AssignsWorkspaceAdmin()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 1 };
        var (db, source, _, _) = await SeedAsync(currentUser, nameof(Handle_WithAdminUserId_AssignsWorkspaceAdmin));
        await using var _ = db;

        var role = new Role { OrganizationId = currentUser.OrganizationId, Name = "Member", Permissions = "workspaces.read" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var admin = new User { OrganizationId = currentUser.OrganizationId, Name = "New Admin", Email = "newadmin@acme.test", PasswordHash = "x", RoleId = role.Id, IsActive = true, IsEmailVerified = true };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var handler = new CopyWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new CopyWorkspaceCommand(source.Id, "Copied Workspace", CopyApps: false, CopyTables: false, AdminUserId: admin.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var membership = Assert.Single(db.WorkspaceMemberships.Where(m => m.WorkspaceId == result.Value));
        Assert.Equal(admin.Id, membership.UserId);
        Assert.Equal(WorkspaceAdminRole.WorkspaceAdmin, membership.Role);
    }

    [Fact]
    public async Task Handle_SourceWorkspaceDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_SourceWorkspaceDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new CopyWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new CopyWorkspaceCommand(999, "Copied Workspace", CopyApps: true, CopyTables: true, AdminUserId: null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
