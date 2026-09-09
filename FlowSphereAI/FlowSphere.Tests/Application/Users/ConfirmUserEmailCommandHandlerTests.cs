using FlowSphere.Application.Users.Commands.ConfirmUserEmail;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Users;

public class ConfirmUserEmailCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, User User)> SeedPendingUserAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var user = new User
        {
            OrganizationId = currentUser.OrganizationId,
            Name = "John Doe",
            Email = "john@acme.test",
            PasswordHash = "x",
            RoleId = null,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = "token123",
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_PendingUser_MarksVerifiedAndClearsToken()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, user) = await SeedPendingUserAsync(currentUser, nameof(Handle_PendingUser_MarksVerifiedAndClearsToken));
        await using var _ = db;

        var handler = new ConfirmUserEmailCommandHandler(db);
        var result = await handler.Handle(new ConfirmUserEmailCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updated = db.Users.Single(u => u.Id == user.Id);
        Assert.True(updated.IsEmailVerified);
        Assert.Null(updated.EmailVerificationToken);
        Assert.Null(updated.EmailVerificationTokenExpiresAt);
    }

    [Fact]
    public async Task Handle_AlreadyVerified_IsNoOpSuccess()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, user) = await SeedPendingUserAsync(currentUser, nameof(Handle_AlreadyVerified_IsNoOpSuccess));
        await using var _ = db;

        var handler = new ConfirmUserEmailCommandHandler(db);
        await handler.Handle(new ConfirmUserEmailCommand(user.Id), CancellationToken.None);
        var result = await handler.Handle(new ConfirmUserEmailCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, nameof(Handle_UserNotFound_ReturnsNotFound));
        await using var _ = db;

        var handler = new ConfirmUserEmailCommandHandler(db);
        var result = await handler.Handle(new ConfirmUserEmailCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
