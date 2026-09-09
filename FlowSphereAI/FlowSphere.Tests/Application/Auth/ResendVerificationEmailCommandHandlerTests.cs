using FlowSphere.Application.Auth.Commands.ResendVerificationEmail;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FlowSphere.Tests.Application.Auth;

public class ResendVerificationEmailCommandHandlerTests
{
    private static ResendVerificationEmailCommandHandler CreateHandler(
        FlowSphere.Infrastructure.Data.FlowSphereDbContext db, FakeCurrentUserContext currentUser) =>
        new(
            db,
            currentUser,
            Options.Create(new ClientSettings { BaseUrl = "http://test.local" }),
            NullLogger<ResendVerificationEmailCommandHandler>.Instance);

    [Fact]
    public async Task Handle_UnverifiedUser_GeneratesNewToken()
    {
        var currentUser = new FakeCurrentUserContext();
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_UnverifiedUser_GeneratesNewToken));

        var user = new User
        {
            OrganizationId = currentUser.OrganizationId,
            Name = "Alice",
            Email = "alice@acme.test",
            PasswordHash = "irrelevant",
            RoleId = 1,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = "old-token",
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(-1),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var scopedUser = new FakeCurrentUserContext { OrganizationId = currentUser.OrganizationId, UserId = user.Id };
        var result = await CreateHandler(db, scopedUser).Handle(new ResendVerificationEmailCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = db.Users.IgnoreQueryFilters().Single();
        Assert.NotEqual("old-token", reloaded.EmailVerificationToken);
        Assert.True(reloaded.EmailVerificationTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_AlreadyVerifiedUser_IsNoOp()
    {
        var currentUser = new FakeCurrentUserContext();
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_AlreadyVerifiedUser_IsNoOp));

        var user = new User
        {
            OrganizationId = currentUser.OrganizationId,
            Name = "Alice",
            Email = "alice@acme.test",
            PasswordHash = "irrelevant",
            RoleId = 1,
            IsActive = true,
            IsEmailVerified = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var scopedUser = new FakeCurrentUserContext { OrganizationId = currentUser.OrganizationId, UserId = user.Id };
        var result = await CreateHandler(db, scopedUser).Handle(new ResendVerificationEmailCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = db.Users.IgnoreQueryFilters().Single();
        Assert.Null(reloaded.EmailVerificationToken);
    }
}
