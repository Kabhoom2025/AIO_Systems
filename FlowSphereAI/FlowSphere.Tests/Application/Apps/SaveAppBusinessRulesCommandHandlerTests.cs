using FlowSphere.Application.Apps.Commands.SaveAppBusinessRules;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SaveAppBusinessRulesCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, int AppId)> SeedAppAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(CancellationToken.None);

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync(CancellationToken.None);

        return (db, app.Id);
    }

    [Fact]
    public async Task Handle_ValidRules_SavesBusinessRulesJson()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ValidRules_SavesBusinessRulesJson));
        await using var _ = db;

        var handler = new SaveAppBusinessRulesCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var rulesJson = """[{"id":"r1","name":"Hide notes unless urgent","enabled":true,"kind":"Rule","event":"ValueChange","triggerFieldKey":"priority","branches":[{"conditions":[{"fieldKey":"priority","operator":"Equals","compareType":"Value","value":"Urgent"}],"actions":[{"level":"Element","targetKey":"notes","action":"Show"}]}]}]""";

        var result = await handler.Handle(new SaveAppBusinessRulesCommand(appId, rulesJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(appId);
        Assert.Equal(rulesJson, saved!.BusinessRulesJson);
    }

    [Fact]
    public async Task Handle_RuleMissingBranches_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_RuleMissingBranches_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppBusinessRulesCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppBusinessRulesCommand(appId, """[{"id":"r1","name":"Bad rule","kind":"Rule","event":"ValueChange"}]"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NotAnArray_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_NotAnArray_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppBusinessRulesCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppBusinessRulesCommand(appId, "{}"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AppBelongsToAnotherOrganization_ReturnsNotFound()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_AppBelongsToAnotherOrganization_ReturnsNotFound);
        var (dbA, appId) = await SeedAppAsync(orgA, dbName);
        await dbA.DisposeAsync();

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var handler = new SaveAppBusinessRulesCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new SaveAppBusinessRulesCommand(appId, "[]"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
