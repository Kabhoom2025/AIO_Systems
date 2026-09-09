using FlowSphere.Application.Apps.Commands.CreateApp;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class CreateAppCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesApp_InsideOwnedWorkspace()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 9 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_CreatesApp_InsideOwnedWorkspace));

        var workspace = Workspace.Create(1, "Sales Ops", null, 9);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new CreateAppCommand(workspace.Id, "Lead Intake", "desc"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(result.Value);
        Assert.NotNull(saved);
        Assert.Equal(workspace.Id, saved!.WorkspaceId);
        Assert.Equal("Lead Intake", saved.Name);
        Assert.Equal("{\"fields\":[]}", saved.FormSchemaJson);
    }

    [Fact]
    public async Task Handle_WorkspaceBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_WorkspaceBelongsToAnotherOrganization_ReturnsNotFound);
        int workspaceId;
        await using (var dbA = TestDbContextFactory.Create(orgA, dbName))
        {
            var workspace = Workspace.Create(1, "Org A Workspace", null, 1);
            dbA.Workspaces.Add(workspace);
            await dbA.SaveChangesAsync(CancellationToken.None);
            workspaceId = workspace.Id;
        }

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new CreateAppCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new CreateAppCommand(workspaceId, "Sneaky App", null), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
