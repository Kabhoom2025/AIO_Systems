using FlowSphere.Application.Auth.Commands.ChangePassword;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Tests.Application.Auth;

public class ChangePasswordCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, FakeCurrentUserContext currentUser)> SeedAsync(string dbName)
    {
        var currentUser = new FakeCurrentUserContext();
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var user = new User
        {
            OrganizationId = currentUser.OrganizationId,
            Name = "Alice",
            Email = "alice@acme.test",
            PasswordHash = new FakePasswordHasher().Hash("OldPassword123"),
            RoleId = 1,
            IsActive = true,
            IsEmailVerified = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var withCorrectUserId = new FakeCurrentUserContext { OrganizationId = currentUser.OrganizationId, UserId = user.Id };
        return (db, withCorrectUserId);
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ReturnsConflictNotUnauthorized()
    {
        var (db, currentUser) = await SeedAsync(nameof(Handle_WrongCurrentPassword_ReturnsConflictNotUnauthorized));
        await using var _ = db;

        var handler = new ChangePasswordCommandHandler(db, currentUser, new FakePasswordHasher());
        var result = await handler.Handle(new ChangePasswordCommand("WrongPassword", "NewPassword123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        // Deliberately Conflict (409), not Unauthorized (401) - apiFetch treats 401 as an expired
        // session and force-logs the user out, which would be wrong for "you mistyped your
        // current password."
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_CorrectCurrentPassword_UpdatesHash()
    {
        var (db, currentUser) = await SeedAsync(nameof(Handle_CorrectCurrentPassword_UpdatesHash));
        await using var _ = db;

        var handler = new ChangePasswordCommandHandler(db, currentUser, new FakePasswordHasher());
        var result = await handler.Handle(new ChangePasswordCommand("OldPassword123", "NewPassword123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == currentUser.UserId);
        Assert.True(new FakePasswordHasher().Verify("NewPassword123", user!.PasswordHash));
    }
}
