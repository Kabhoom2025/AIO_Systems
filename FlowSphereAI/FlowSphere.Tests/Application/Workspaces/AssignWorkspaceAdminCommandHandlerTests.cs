using FlowSphere.Application.Workspaces.Commands.AssignWorkspaceAdmin;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Workspaces;

public class AssignWorkspaceAdminCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Workspace, User User)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var role = new Role { OrganizationId = currentUser.OrganizationId, Name = "Member", Permissions = "workspaces.read" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        var user = new User { OrganizationId = currentUser.OrganizationId, Name = "Jane Admin", Email = "jane@acme.test", PasswordHash = "x", RoleId = role.Id, IsActive = true, IsEmailVerified = true };
        db.Workspaces.Add(workspace);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, workspace, user);
    }

    [Fact]
    public async Task Handle_ValidAssignment_CreatesMembership()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, user) = await SeedAsync(currentUser, nameof(Handle_ValidAssignment_CreatesMembership));
        await using var _ = db;

        var handler = new AssignWorkspaceAdminCommandHandler(db, currentUser);
        var result = await handler.Handle(new AssignWorkspaceAdminCommand(workspace.Id, user.Id, WorkspaceAdminRole.WorkspaceAdmin), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.WorkspaceMemberships);
    }

    [Fact]
    public async Task Handle_AssignedTwice_DoesNotDuplicate()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, user) = await SeedAsync(currentUser, nameof(Handle_AssignedTwice_DoesNotDuplicate));
        await using var _ = db;

        var handler = new AssignWorkspaceAdminCommandHandler(db, currentUser);
        await handler.Handle(new AssignWorkspaceAdminCommand(workspace.Id, user.Id, WorkspaceAdminRole.WorkspaceAdmin), CancellationToken.None);
        var result = await handler.Handle(new AssignWorkspaceAdminCommand(workspace.Id, user.Id, WorkspaceAdminRole.WorkspaceAdmin), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.WorkspaceMemberships);
    }

    [Fact]
    public async Task Handle_WorkspaceDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, user) = await SeedAsync(currentUser, nameof(Handle_WorkspaceDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new AssignWorkspaceAdminCommandHandler(db, currentUser);
        var result = await handler.Handle(new AssignWorkspaceAdminCommand(999, user.Id, WorkspaceAdminRole.DataAdmin), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
