using FlowSphere.Application.Apps.Queries.GetAppPublishHistory;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class GetAppPublishHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEntriesForAppOnly_NewestFirst()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ReturnsEntriesForAppOnly_NewestFirst));

        var workspace = Workspace.Create(1, "Sales Ops", null, 1);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var appA = AppDefinition.Create(workspace.Id, "App A", null, 1);
        var appB = AppDefinition.Create(workspace.Id, "App B", null, 1);
        db.AppDefinitions.AddRange(appA, appB);
        await db.SaveChangesAsync();

        db.AppPublishHistoryEntries.AddRange(
            new AppPublishHistoryEntry { OrganizationId = 1, AppId = appA.Id, Comment = "First", PublishedByUserId = 1, PublishedByUserName = "Ada" },
            new AppPublishHistoryEntry { OrganizationId = 1, AppId = appA.Id, Comment = "Second", PublishedByUserId = 1, PublishedByUserName = "Ada" },
            new AppPublishHistoryEntry { OrganizationId = 1, AppId = appB.Id, Comment = "Unrelated", PublishedByUserId = 1, PublishedByUserName = "Ada" });
        await db.SaveChangesAsync();

        var handler = new GetAppPublishHistoryQueryHandler(db);
        var result = await handler.Handle(new GetAppPublishHistoryQuery(appA.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, e => Assert.True(e.Comment == "First" || e.Comment == "Second"));
    }
}
