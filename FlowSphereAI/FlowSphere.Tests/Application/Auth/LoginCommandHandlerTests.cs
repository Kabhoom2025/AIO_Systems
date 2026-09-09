using FlowSphere.Application.Auth.Commands.Login;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Tests.Application.Auth;

public class LoginCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, User user)> SeedUserAsync(
        string dbName, FakeCurrentUserContext currentUser, bool isActive = true)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var org = new Organization { Name = "Acme Corp" };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var role = new Role { OrganizationId = org.Id, Name = "Admin", Permissions = "workflows.read" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User
        {
            OrganizationId = org.Id,
            Name = "Alice Admin",
            Email = "alice@acme.test",
            PasswordHash = new FakePasswordHasher().Hash("Password123"),
            RoleId = role.Id,
            IsActive = isActive,
            IsEmailVerified = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_ValidCredentials_SucceedsAndSetsLastLoginAt()
    {
        var currentUser = new FakeCurrentUserContext();
        var (db, user) = await SeedUserAsync(nameof(Handle_ValidCredentials_SucceedsAndSetsLastLoginAt), currentUser);
        await using var _ = db;

        var handler = new LoginCommandHandler(db, new FakePasswordHasher(), new FakeJwtTokenGenerator());
        var result = await handler.Handle(new LoginCommand("alice@acme.test", "Password123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsEmailVerified);

        var reloaded = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(reloaded!.LastLoginAt);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        var currentUser = new FakeCurrentUserContext();
        var (db, _) = await SeedUserAsync(nameof(Handle_WrongPassword_ReturnsUnauthorized), currentUser);
        await using var _ = db;

        var handler = new LoginCommandHandler(db, new FakePasswordHasher(), new FakeJwtTokenGenerator());
        var result = await handler.Handle(new LoginCommand("alice@acme.test", "WrongPassword"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_DeactivatedUser_ReturnsUnauthorized()
    {
        var currentUser = new FakeCurrentUserContext();
        var (db, _) = await SeedUserAsync(nameof(Handle_DeactivatedUser_ReturnsUnauthorized), currentUser, isActive: false);
        await using var _ = db;

        var handler = new LoginCommandHandler(db, new FakePasswordHasher(), new FakeJwtTokenGenerator());
        var result = await handler.Handle(new LoginCommand("alice@acme.test", "Password123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }
}
