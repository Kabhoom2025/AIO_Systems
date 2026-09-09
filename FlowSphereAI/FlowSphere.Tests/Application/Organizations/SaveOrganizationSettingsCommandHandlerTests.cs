using FlowSphere.Application.Organizations.Commands.SaveOrganizationSettings;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Organizations;

public class SaveOrganizationSettingsCommandHandlerTests
{
    private static async Task<FlowSphere.Infrastructure.Data.FlowSphereDbContext> SeedAsync(FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = currentUser.OrganizationId, Name = "Acme Corp", MonthlyExecutionQuota = 100 });
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Handle_ValidBackgroundImage_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidBackgroundImage_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var settingsJson = """{"backgroundImageDataUri":"data:image/png;base64,iVBORw0KGgo="}""";
        var result = await handler.Handle(new SaveOrganizationSettingsCommand(settingsJson), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var org = Assert.Single(db.Organizations);
        Assert.Equal(settingsJson, org.SettingsJson);
    }

    [Fact]
    public async Task Handle_NonImageDataUri_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_NonImageDataUri_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new SaveOrganizationSettingsCommand("""{"backgroundImageDataUri":"not-a-data-uri"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_OversizedImage_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_OversizedImage_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var hugeValue = "data:image/png;base64," + new string('A', 9_000_000);
        var result = await handler.Handle(
            new SaveOrganizationSettingsCommand($$"""{"backgroundImageDataUri":"{{hugeValue}}"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NullBackgroundImage_ClearsIt()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_NullBackgroundImage_ClearsIt));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"backgroundImageDataUri":null}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidGlossyThemeFlag_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidGlossyThemeFlag_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"glossyThemeEnabled":true}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_NonBooleanGlossyThemeFlag_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_NonBooleanGlossyThemeFlag_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"glossyThemeEnabled":"yes"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidColorTheme_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidColorTheme_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"colorTheme":"ocean"}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UnknownColorTheme_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_UnknownColorTheme_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"colorTheme":"neon"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidFontFamily_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidFontFamily_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"fontFamily":"serif"}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UnknownFontFamily_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_UnknownFontFamily_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"fontFamily":"comic-sans"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidFontScale_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidFontScale_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"fontScale":1.125}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_OutOfRangeFontScale_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_OutOfRangeFontScale_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"fontScale":2.5}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidCustomColors_Persists()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_ValidCustomColors_Persists));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new SaveOrganizationSettingsCommand("""{"customColors":{"header":"#112233","sidebar":"#445566","body":null}}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_InvalidCustomColorHex_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_InvalidCustomColorHex_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new SaveOrganizationSettingsCommand("""{"customColors":{"header":"not-a-color"}}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CustomColorsNotAnObject_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_CustomColorsNotAnObject_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(new SaveOrganizationSettingsCommand("""{"customColors":"#112233"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    /// <summary>Regression test - IsValidSettings used to `return true` as soon as
    /// backgroundImageDataUri was null, short-circuiting validation of every other property in
    /// the same payload.</summary>
    [Fact]
    public async Task Handle_NullImageWithInvalidGlossyFlag_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = await SeedAsync(currentUser, nameof(Handle_NullImageWithInvalidGlossyFlag_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SaveOrganizationSettingsCommandHandler(db, currentUser);
        var result = await handler.Handle(
            new SaveOrganizationSettingsCommand("""{"backgroundImageDataUri":null,"glossyThemeEnabled":"not-a-bool"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
