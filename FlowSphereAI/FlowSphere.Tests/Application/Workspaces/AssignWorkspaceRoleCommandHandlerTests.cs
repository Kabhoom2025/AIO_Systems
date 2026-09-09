using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.AssignWorkspaceRole;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Workspaces;

public class AssignWorkspaceRoleCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Workspace, WorkspaceRole Role, User User)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var globalRole = new Role { OrganizationId = currentUser.OrganizationId, Name = "Employee", Permissions = "apps.read" };
        db.Roles.Add(globalRole);
        await db.SaveChangesAsync();

        var workspace = Workspace.Create(currentUser.OrganizationId, "HRMS", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var workspaceRole = new WorkspaceRole { WorkspaceId = workspace.Id, Name = "Billing Team", Permissions = PermissionCatalog.AppsWrite };
        db.WorkspaceRoles.Add(workspaceRole);

        var user = new User
        {
            OrganizationId = currentUser.OrganizationId, Name = "Jordan", Email = "jordan@acme.test",
            PasswordHash = "x", RoleId = globalRole.Id, IsActive = true, IsEmailVerified = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, workspace, workspaceRole, user);
    }

    [Fact]
    public async Task Handle_ValidAssignment_CreatesAssignment()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, role, user) = await SeedAsync(currentUser, nameof(Handle_ValidAssignment_CreatesAssignment));
        await using var _ = db;

        var handler = new AssignWorkspaceRoleCommandHandler(db, currentUser);
        var result = await handler.Handle(new AssignWorkspaceRoleCommand(workspace.Id, user.Id, role.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.WorkspaceRoleAssignments);
    }

    [Fact]
    public async Task Handle_AssignedTwice_DoesNotDuplicate()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, role, user) = await SeedAsync(currentUser, nameof(Handle_AssignedTwice_DoesNotDuplicate));
        await using var _ = db;

        var handler = new AssignWorkspaceRoleCommandHandler(db, currentUser);
        await handler.Handle(new AssignWorkspaceRoleCommand(workspace.Id, user.Id, role.Id), CancellationToken.None);
        var result = await handler.Handle(new AssignWorkspaceRoleCommand(workspace.Id, user.Id, role.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.WorkspaceRoleAssignments);
    }

    [Fact]
    public async Task Handle_RoleNotInWorkspace_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, role, user) = await SeedAsync(currentUser, nameof(Handle_RoleNotInWorkspace_ReturnsNotFound));
        await using var _ = db;

        var otherWorkspace = Workspace.Create(currentUser.OrganizationId, "Other", null, currentUser.UserId);
        db.Workspaces.Add(otherWorkspace);
        await db.SaveChangesAsync();

        var handler = new AssignWorkspaceRoleCommandHandler(db, currentUser);
        var result = await handler.Handle(new AssignWorkspaceRoleCommand(otherWorkspace.Id, user.Id, role.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
