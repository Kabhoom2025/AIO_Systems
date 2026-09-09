using FlowSphere.Application.Apps.Commands.SaveAppAccessPermissions;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SaveAppAccessPermissionsCommandHandlerTests
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
    public async Task Handle_ValidPermissions_SavesAccessPermissionsJson()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ValidPermissions_SavesAccessPermissionsJson));
        await using var _ = db;

        var handler = new SaveAppAccessPermissionsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var permissionsJson = """{"step1":{"salary":"Hidden","notes":"Readonly"},"step2":{"salary":"Custom"}}""";

        var result = await handler.Handle(new SaveAppAccessPermissionsCommand(appId, permissionsJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(appId);
        Assert.Equal(permissionsJson, saved!.AccessPermissionsJson);
    }

    [Fact]
    public async Task Handle_UnknownPermissionState_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_UnknownPermissionState_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppAccessPermissionsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppAccessPermissionsCommand(appId, """{"step1":{"salary":"SuperSecret"}}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_StepValueNotAnObject_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_StepValueNotAnObject_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppAccessPermissionsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        // Old flat shape (fieldKey -> state directly) must be rejected now that a step level is required.
        var result = await handler.Handle(new SaveAppAccessPermissionsCommand(appId, """{"salary":"Hidden"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NotAnObject_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_NotAnObject_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppAccessPermissionsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppAccessPermissionsCommand(appId, "[]"), CancellationToken.None);

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
        var handler = new SaveAppAccessPermissionsCommandHandler(dbB, orgB, new FakeCurrentEnvironmentContext());

        var result = await handler.Handle(new SaveAppAccessPermissionsCommand(appId, "{}"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
