using FlowSphere.Application.Common;
using FlowSphere.Application.Users.Commands.SetUserActive;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Users;

public class SetUserActiveCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext db, Role adminRole, User admin1, User admin2, User member)>
        SeedAsync(string dbName, FakeCurrentUserContext currentUser)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);

        var adminRole = new Role { OrganizationId = currentUser.OrganizationId, Name = "Admin", Permissions = "users.manage" };
        var memberRole = new Role { OrganizationId = currentUser.OrganizationId, Name = "Member", Permissions = "workflows.read" };
        db.Roles.AddRange(adminRole, memberRole);
        await db.SaveChangesAsync();

        var admin1 = new User { OrganizationId = currentUser.OrganizationId, Name = "Admin One", Email = "admin1@acme.test", PasswordHash = "x", RoleId = adminRole.Id, IsActive = true, IsEmailVerified = true };
        var admin2 = new User { OrganizationId = currentUser.OrganizationId, Name = "Admin Two", Email = "admin2@acme.test", PasswordHash = "x", RoleId = adminRole.Id, IsActive = true, IsEmailVerified = true };
        var member = new User { OrganizationId = currentUser.OrganizationId, Name = "Member One", Email = "member1@acme.test", PasswordHash = "x", RoleId = memberRole.Id, IsActive = true, IsEmailVerified = true };
        db.Users.AddRange(admin1, admin2, member);
        await db.SaveChangesAsync();

        return (db, adminRole, admin1, admin2, member);
    }

    [Fact]
    public async Task Handle_SelfDeactivation_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, _, _) = await SeedAsync(nameof(Handle_SelfDeactivation_ReturnsConflict), currentUser);
        await using var _ = db;

        var actingUser = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var result = await new SetUserActiveCommandHandler(db, actingUser)
            .Handle(new SetUserActiveCommand(admin1.Id, false), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_DeactivateLastAdmin_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, admin2, _) = await SeedAsync(nameof(Handle_DeactivateLastAdmin_ReturnsConflict), currentUser);
        await using var _ = db;

        // Deactivate admin2 first via admin1 (fine, 2 admins -> 1) then try to deactivate the
        // now-only admin (admin1) via a hypothetical other actor - simulated by acting as admin2
        // even though admin2 is now inactive, to isolate the "last admin" guard from "self" guard.
        var actingAsAdmin1 = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var first = await new SetUserActiveCommandHandler(db, actingAsAdmin1)
            .Handle(new SetUserActiveCommand(admin2.Id, false), CancellationToken.None);
        Assert.True(first.IsSuccess);

        var actingAsAdmin2 = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin2.Id };
        var second = await new SetUserActiveCommandHandler(db, actingAsAdmin2)
            .Handle(new SetUserActiveCommand(admin1.Id, false), CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorType.Conflict, second.Error!.Type);
        Assert.Equal("Cannot deactivate the last admin in the organization.", second.Error.Message);
    }

    [Fact]
    public async Task Handle_DeactivateMember_Succeeds()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, _, member) = await SeedAsync(nameof(Handle_DeactivateMember_Succeeds), currentUser);
        await using var _ = db;

        var actingUser = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var result = await new SetUserActiveCommandHandler(db, actingUser)
            .Handle(new SetUserActiveCommand(member.Id, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = db.Users.Single(u => u.Id == member.Id);
        Assert.False(reloaded.IsActive);
    }
}
