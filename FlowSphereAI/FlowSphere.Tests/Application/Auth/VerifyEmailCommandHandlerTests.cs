using FlowSphere.Application.Auth.Commands.VerifyEmail;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Tests.Application.Auth;

public class VerifyEmailCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, User user)> SeedUnverifiedUserAsync(
        string dbName, DateTime? tokenExpiresAt = null)
    {
        var currentUser = new FakeCurrentUserContext();
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var user = new User
        {
            OrganizationId = 1,
            Name = "Alice",
            Email = "alice@acme.test",
            PasswordHash = "irrelevant",
            RoleId = 1,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = "valid-token",
            EmailVerificationTokenExpiresAt = tokenExpiresAt ?? DateTime.UtcNow.AddHours(24),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_ValidToken_MarksVerifiedAndClearsToken()
    {
        var (db, _) = await SeedUnverifiedUserAsync(nameof(Handle_ValidToken_MarksVerifiedAndClearsToken));
        await using var _ = db;

        var result = await new VerifyEmailCommandHandler(db).Handle(new VerifyEmailCommand("valid-token"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var user = db.Users.IgnoreQueryFilters().Single();
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationToken);
        Assert.Null(user.EmailVerificationTokenExpiresAt);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsNotFound()
    {
        var (db, _) = await SeedUnverifiedUserAsync(nameof(Handle_UnknownToken_ReturnsNotFound));
        await using var _ = db;

        var result = await new VerifyEmailCommandHandler(db).Handle(new VerifyEmailCommand("garbage-token"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsNotFound()
    {
        var (db, _) = await SeedUnverifiedUserAsync(
            nameof(Handle_ExpiredToken_ReturnsNotFound), DateTime.UtcNow.AddHours(-1));
        await using var _ = db;

        var result = await new VerifyEmailCommandHandler(db).Handle(new VerifyEmailCommand("valid-token"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
