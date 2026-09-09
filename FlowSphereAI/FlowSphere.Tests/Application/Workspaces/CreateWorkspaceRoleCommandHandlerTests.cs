using FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.CreateWorkspaceRole;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Workspaces;

public class CreateWorkspaceRoleCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Workspace)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "HRMS", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();
        return (db, workspace);
    }

    [Fact]
    public async Task Handle_ValidRole_CreatesWorkspaceRole()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace) = await SeedAsync(currentUser, nameof(Handle_ValidRole_CreatesWorkspaceRole));
        await using var _ = db;

        var handler = new CreateWorkspaceRoleCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new CreateWorkspaceRoleCommand(workspace.Id, "Billing Team", new List<string> { PermissionCatalog.AppsWrite }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var role = db.WorkspaceRoles.Single();
        Assert.Equal("Billing Team", role.Name);
        Assert.Contains(PermissionCatalog.AppsWrite, role.PermissionList);
    }

    [Fact]
    public async Task Handle_DuplicateNameInSameWorkspace_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace) = await SeedAsync(currentUser, nameof(Handle_DuplicateNameInSameWorkspace_ReturnsConflict));
        await using var _ = db;

        var handler = new CreateWorkspaceRoleCommandHandler(db, currentUser);
        await handler.Handle(new CreateWorkspaceRoleCommand(workspace.Id, "Billing Team", new List<string> { PermissionCatalog.AppsWrite }), CancellationToken.None);
        var result = await handler.Handle(new CreateWorkspaceRoleCommand(workspace.Id, "Billing Team", new List<string> { PermissionCatalog.TablesWrite }), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WorkspaceDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_WorkspaceDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new CreateWorkspaceRoleCommandHandler(db, currentUser);
        var result = await handler.Handle(new CreateWorkspaceRoleCommand(999, "Billing Team", new List<string> { PermissionCatalog.AppsWrite }), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
