using FlowSphere.Application.Workspaces.Commands.CreateWorkspace;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Workspaces;

public class CreateWorkspaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesWorkspace_ScopedToCallerOrganization()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 42, UserId = 7 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_CreatesWorkspace_ScopedToCallerOrganization));
        var handler = new CreateWorkspaceCommandHandler(db, currentUser);

        var result = await handler.Handle(new CreateWorkspaceCommand("Sales Ops", "desc"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Workspaces.FindAsync(result.Value);
        Assert.NotNull(saved);
        Assert.Equal(42, saved!.OrganizationId);
        Assert.Equal("Sales Ops", saved.Name);
        Assert.Equal(7, saved.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_DoesNotLeakWorkspaces_AcrossOrganizations()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_DoesNotLeakWorkspaces_AcrossOrganizations);
        await using (var dbA = TestDbContextFactory.Create(orgA, dbName))
        {
            await new CreateWorkspaceCommandHandler(dbA, orgA)
                .Handle(new CreateWorkspaceCommand("Org A Workspace", null), CancellationToken.None);
        }

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var orgBWorkspaces = dbB.Workspaces.ToList();

        Assert.Empty(orgBWorkspaces);
    }
}
