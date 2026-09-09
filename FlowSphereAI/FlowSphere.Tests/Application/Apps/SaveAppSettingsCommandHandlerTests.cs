using FlowSphere.Application.Apps.Commands.SaveAppSettings;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SaveAppSettingsCommandHandlerTests
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
    public async Task Handle_ValidSettings_SavesSettingsJson()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_ValidSettings_SavesSettingsJson));
        await using var _ = db;

        var handler = new SaveAppSettingsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var settingsJson = """{"appOpenLink":"AppData","redirectMode":"Home","sectionsDisplay":"Tabs"}""";

        var result = await handler.Handle(new SaveAppSettingsCommand(appId, settingsJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.AppDefinitions.FindAsync(appId);
        Assert.Equal(settingsJson, saved!.SettingsJson);
    }

    [Fact]
    public async Task Handle_UnknownRedirectMode_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_UnknownRedirectMode_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveAppSettingsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppSettingsCommand(appId, """{"redirectMode":"Teleport"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_EmptyObject_IsValid()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, appId) = await SeedAppAsync(currentUser, nameof(Handle_EmptyObject_IsValid));
        await using var _ = db;

        var handler = new SaveAppSettingsCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new SaveAppSettingsCommand(appId, "{}"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
