using FlowSphere.Application.Apps.Commands.SaveAppForm;
using FlowSphere.Application.Behaviors;
using FlowSphere.Application.Common;
using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.AssignWorkspaceRole;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Behaviors;

public class WorkspacePermissionBehaviorTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Workspace, AppDefinition App)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "HRMS", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Leave Request", null, currentUser.UserId);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, workspace, app);
    }

    [Fact]
    public async Task Handle_UserWithGlobalPermission_AlwaysSucceeds()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 1, Permissions = new[] { PermissionCatalog.AppsWrite } };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_UserWithGlobalPermission_AlwaysSucceeds));
        await using var _ = db;

        var behavior = new WorkspacePermissionBehavior<SaveAppFormCommand, Result>(currentUser, db);
        var called = false;
        var result = await behavior.Handle(new SaveAppFormCommand(app.Id, "{}"), () =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserWithWorkspaceRoleGrant_Succeeds()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 2, Permissions = Array.Empty<string>() };
        var (db, workspace, app) = await SeedAsync(currentUser, nameof(Handle_UserWithWorkspaceRoleGrant_Succeeds));
        await using var _ = db;

        var workspaceRole = new WorkspaceRole { WorkspaceId = workspace.Id, Name = "Billing Team", Permissions = PermissionCatalog.AppsWrite };
        db.WorkspaceRoles.Add(workspaceRole);
        await db.SaveChangesAsync();
        db.WorkspaceRoleAssignments.Add(new WorkspaceRoleAssignment { WorkspaceId = workspace.Id, UserId = currentUser.UserId, WorkspaceRoleId = workspaceRole.Id });
        await db.SaveChangesAsync();

        var behavior = new WorkspacePermissionBehavior<SaveAppFormCommand, Result>(currentUser, db);
        var called = false;
        var result = await behavior.Handle(new SaveAppFormCommand(app.Id, "{}"), () =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserWithNeitherGlobalNorWorkspaceGrant_IsRejected()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 3, Permissions = Array.Empty<string>() };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_UserWithNeitherGlobalNorWorkspaceGrant_IsRejected));
        await using var _ = db;

        var behavior = new WorkspacePermissionBehavior<SaveAppFormCommand, Result>(currentUser, db);
        var called = false;
        var result = await behavior.Handle(new SaveAppFormCommand(app.Id, "{}"), () =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.False(called);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserWithGrantInDifferentWorkspace_IsRejected()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 4, Permissions = Array.Empty<string>() };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_UserWithGrantInDifferentWorkspace_IsRejected));
        await using var _ = db;

        var otherWorkspace = Workspace.Create(currentUser.OrganizationId, "Other", null, currentUser.UserId);
        db.Workspaces.Add(otherWorkspace);
        await db.SaveChangesAsync();

        var workspaceRole = new WorkspaceRole { WorkspaceId = otherWorkspace.Id, Name = "Billing Team", Permissions = PermissionCatalog.AppsWrite };
        db.WorkspaceRoles.Add(workspaceRole);
        await db.SaveChangesAsync();
        db.WorkspaceRoleAssignments.Add(new WorkspaceRoleAssignment { WorkspaceId = otherWorkspace.Id, UserId = currentUser.UserId, WorkspaceRoleId = workspaceRole.Id });
        await db.SaveChangesAsync();

        var behavior = new WorkspacePermissionBehavior<SaveAppFormCommand, Result>(currentUser, db);
        var result = await behavior.Handle(new SaveAppFormCommand(app.Id, "{}"), () => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NonScopedRequest_PassesThroughUnaffected()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 5, Permissions = Array.Empty<string>() };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NonScopedRequest_PassesThroughUnaffected));
        await using var _ = db;

        // AssignWorkspaceRoleCommand doesn't implement any of the scoped-request marker
        // interfaces, so the behavior must be a pure pass-through regardless of permissions.
        var behavior = new WorkspacePermissionBehavior<AssignWorkspaceRoleCommand, Result>(currentUser, db);
        var called = false;
        var result = await behavior.Handle(new AssignWorkspaceRoleCommand(1, 1, 1), () =>
        {
            called = true;
            return Task.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }
}
