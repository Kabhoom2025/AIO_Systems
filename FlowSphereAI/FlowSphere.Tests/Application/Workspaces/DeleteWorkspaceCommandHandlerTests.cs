using FlowSphere.Application.Workspaces.Commands.DeleteWorkspace;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Workspaces;

public class DeleteWorkspaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingWorkspace_CascadeDeletesAppsAndTables()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExistingWorkspace_CascadeDeletesAppsAndTables));
        await using var _ = db;

        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        db.AppDefinitions.Add(AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId));
        db.TableDefinitions.Add(TableDefinition.Create(workspace.Id, "Leads", null, currentUser.UserId));
        await db.SaveChangesAsync();

        var handler = new DeleteWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(new DeleteWorkspaceCommand(workspace.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(await db.Workspaces.FindAsync(workspace.Id));
        Assert.Empty(db.AppDefinitions);
        Assert.Empty(db.TableDefinitions);
    }

    [Fact]
    public async Task Handle_WorkspaceDoesNotExist_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_WorkspaceDoesNotExist_ReturnsNotFound));
        await using var _ = db;

        var handler = new DeleteWorkspaceCommandHandler(db, currentUser);
        var result = await handler.Handle(new DeleteWorkspaceCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
