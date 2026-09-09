using FlowSphere.Application.Apps.Commands.SaveAppForm;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SaveAppFormCommandHandlerTests
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
    public async Task Handle_ValidSchema_SavesFormSchemaJson()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ValidSchema_SavesFormSchemaJson));
        await using var _ = db;

        var handler = new SaveAppFormCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var schemaJson = """{"fields":[{"key":"name","type":"Text","label":"Name","required":true}]}""";

        var result = await handler.Handle(new SaveAppFormCommand(appId, schemaJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(appId);
        Assert.Equal(schemaJson, saved!.FormSchemaJson);
    }

    [Fact]
    public async Task Handle_SchemaMissingFieldsArray_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_SchemaMissingFieldsArray_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppFormCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppFormCommand(appId, """{"notFields":[]}"""), CancellationToken.None);

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
        var handler = new SaveAppFormCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new SaveAppFormCommand(appId, """{"fields":[]}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
