using FlowSphere.Application.Apps.Commands.SaveAppUserManual;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SaveAppUserManualCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidMarkdown_SavesUserManual()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ValidMarkdown_SavesUserManual));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, 1);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new SaveAppUserManualCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppUserManualCommand(app.Id, "# How to use this app"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.Equal("# How to use this app", saved!.UserManualMarkdown);
    }

    [Fact]
    public async Task Handle_NullMarkdown_ClearsUserManual()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NullMarkdown_ClearsUserManual));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, 1);
        app.UpdateUserManual("Old content");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new SaveAppUserManualCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppUserManualCommand(app.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.Null(saved!.UserManualMarkdown);
    }
}
