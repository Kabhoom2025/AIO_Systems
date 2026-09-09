using FlowSphere.Application.Tables.Commands.CreateTable;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Tables;

public class CreateTableCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesTable_InsideOwnedWorkspace()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 9 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_CreatesTable_InsideOwnedWorkspace));

        var workspace = Workspace.Create(1, "Sales Ops", null, 9);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateTableCommandHandler(db, currentUser);
        var result = await handler.Handle(new CreateTableCommand(workspace.Id, "Leads", "desc"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.TableDefinitions.FindAsync(result.Value);
        Assert.NotNull(saved);
        Assert.Equal(workspace.Id, saved!.WorkspaceId);
        Assert.Equal("Leads", saved.Name);
        Assert.Equal("{\"columns\":[]}", saved.SchemaJson);
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
        var handler = new CreateTableCommandHandler(dbB, orgB);

        var result = await handler.Handle(new CreateTableCommand(workspaceId, "Sneaky Table", null), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
