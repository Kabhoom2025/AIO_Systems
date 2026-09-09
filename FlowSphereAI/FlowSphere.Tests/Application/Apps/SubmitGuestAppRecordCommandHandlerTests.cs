using FlowSphere.Application.Apps.Commands.SubmitGuestAppRecord;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SubmitGuestAppRecordCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, AppDefinition App)> SeedAsync(string dbName, string settingsJson)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = 1, Name = "Acme Corp", MonthlyExecutionQuota = 100 });

        var workspace = Workspace.Create(1, "Public Forms", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Feedback", null, currentUser.UserId);
        app.UpdateFormSchema("""{"fields":[{"key":"email","type":"Text","label":"Email"}]}""");
        app.UpdateSettings(settingsJson);
        app.Publish();
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, app);
    }

    [Fact]
    public async Task Handle_GuestLinkDisabled_ReturnsUnauthorized()
    {
        var (db, app) = await SeedAsync(nameof(Handle_GuestLinkDisabled_ReturnsUnauthorized), "{}");
        await using var _ = db;

        var handler = new SubmitGuestAppRecordCommandHandler(
            db, new FakeCurrentEnvironmentContext(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());

        var result = await handler.Handle(new SubmitGuestAppRecordCommand(app.Id, "any-token", """{"email":"a@b.com"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_WrongToken_ReturnsUnauthorized()
    {
        var settings = """{"shared":{"guestLinkEnabled":true,"guestToken":"correct-token"}}""";
        var (db, app) = await SeedAsync(nameof(Handle_WrongToken_ReturnsUnauthorized), settings);
        await using var _ = db;

        var handler = new SubmitGuestAppRecordCommandHandler(
            db, new FakeCurrentEnvironmentContext(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());

        var result = await handler.Handle(new SubmitGuestAppRecordCommand(app.Id, "wrong-token", """{"email":"a@b.com"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_ValidGuestSubmit_CreatesRecordWithNoSubmitterAndGuestSource()
    {
        var settings = """{"shared":{"guestLinkEnabled":true,"guestToken":"correct-token"}}""";
        var (db, app) = await SeedAsync(nameof(Handle_ValidGuestSubmit_CreatesRecordWithNoSubmitterAndGuestSource), settings);
        await using var _ = db;

        var handler = new SubmitGuestAppRecordCommandHandler(
            db, new FakeCurrentEnvironmentContext(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());

        var result = await handler.Handle(new SubmitGuestAppRecordCommand(app.Id, "correct-token", """{"email":"a@b.com"}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var record = Assert.Single(db.AppRecords);
        Assert.Null(record.CreatedByUserId);
        Assert.Equal(AppRecordSource.Guest, record.Source);
    }

    [Fact]
    public async Task Handle_CaptchaRequiredButWrongAnswer_ReturnsValidationFailure()
    {
        var settings = """{"shared":{"guestLinkEnabled":true,"guestToken":"correct-token"},"accessibility":{"captcha":{"enabled":true,"when":"OnSubmission"}}}""";
        var (db, app) = await SeedAsync(nameof(Handle_CaptchaRequiredButWrongAnswer_ReturnsValidationFailure), settings);
        await using var _ = db;

        var handler = new SubmitGuestAppRecordCommandHandler(
            db, new FakeCurrentEnvironmentContext(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());

        var result = await handler.Handle(new SubmitGuestAppRecordCommand(
            app.Id, "correct-token", """{"email":"a@b.com"}""", null, "wrong-token", 1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_CaptchaRequiredWithCorrectAnswer_Succeeds()
    {
        var settings = """{"shared":{"guestLinkEnabled":true,"guestToken":"correct-token"},"accessibility":{"captcha":{"enabled":true,"when":"OnSubmission"}}}""";
        var (db, app) = await SeedAsync(nameof(Handle_CaptchaRequiredWithCorrectAnswer_Succeeds), settings);
        await using var _ = db;

        var handler = new SubmitGuestAppRecordCommandHandler(
            db, new FakeCurrentEnvironmentContext(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());

        var result = await handler.Handle(new SubmitGuestAppRecordCommand(
            app.Id, "correct-token", """{"email":"a@b.com"}""", null, FakeCaptchaService.ValidToken, FakeCaptchaService.ValidAnswer), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.AppRecords);
    }
}
