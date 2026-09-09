using FlowSphere.Application.Apps.Commands.PublishApp;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class PublishAppCommandHandlerTests
{
    [Fact]
    public async Task Handle_AppHasFields_PublishesSuccessfully()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppHasFields_PublishesSuccessfully));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, 1);
        app.UpdateFormSchema("""{"fields":[{"key":"name","type":"Text","label":"Name","required":true}]}""");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new PublishAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new PublishAppCommand(app.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.True(saved!.IsPublished);
        Assert.NotNull(saved.PublishedAt);
    }

    [Fact]
    public async Task Handle_AppHasFieldsInSectionsShape_PublishesSuccessfully()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppHasFieldsInSectionsShape_PublishesSuccessfully));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, 1);
        app.UpdateFormSchema("""
            {"sections":[{"id":"section_1","title":"Details","fields":[
              {"key":"name","type":"Text","label":"Name","required":true,"layout":{"x":0,"y":0,"w":6,"h":1}}
            ]}]}
            """);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new PublishAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new PublishAppCommand(app.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.True(saved!.IsPublished);
    }

    [Fact]
    public async Task Handle_AppHasEmptySections_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppHasEmptySections_ReturnsValidationFailure));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Empty App", null, 1);
        app.UpdateFormSchema("""{"sections":[{"id":"section_1","title":"Details","fields":[]}]}""");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new PublishAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new PublishAppCommand(app.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.False(saved!.IsPublished);
    }

    [Fact]
    public async Task Handle_AppHasNoFields_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AppHasNoFields_ReturnsValidationFailure));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Empty App", null, 1);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new PublishAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new PublishAppCommand(app.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.False(saved!.IsPublished);
    }

    [Fact]
    public async Task Handle_WithComment_SnapshotsConfigAndInsertsHistoryEntry()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 42 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_WithComment_SnapshotsConfigAndInsertsHistoryEntry));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var user = new FlowSphere.Domain.Entities.User { Id = 42, OrganizationId = 1, Name = "Ada Lovelace", Email = "ada@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, 1);
        app.UpdateFormSchema("""{"fields":[{"key":"name","type":"Text","label":"Name","required":true}]}""");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        var handler = new PublishAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new PublishAppCommand(app.Id, "Initial release"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(app.Id);
        Assert.True(saved!.IsPublished);
        Assert.NotNull(saved.PublishedSnapshotJson);
        Assert.Contains("name", saved.PublishedSnapshotJson);

        var historyEntry = Assert.Single(db.AppPublishHistoryEntries.Where(h => h.AppId == app.Id));
        Assert.Equal("Initial release", historyEntry.Comment);
        Assert.Equal("Ada Lovelace", historyEntry.PublishedByUserName);
        Assert.Equal(42, historyEntry.PublishedByUserId);
    }
}
