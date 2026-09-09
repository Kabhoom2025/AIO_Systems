using FlowSphere.Application.Common;
using FlowSphere.Application.Users.Commands.ChangeUserRole;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Users;

public class ChangeUserRoleCommandHandlerTests
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
    public async Task Handle_ChangeOwnRole_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, _, _) = await SeedAsync(nameof(Handle_ChangeOwnRole_ReturnsConflict), currentUser);
        await using var _ = db;

        var actingUser = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var result = await new ChangeUserRoleCommandHandler(db, actingUser)
            .Handle(new ChangeUserRoleCommand(admin1.Id, "Member"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_DemoteLastAdmin_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, admin2, _) = await SeedAsync(nameof(Handle_DemoteLastAdmin_ReturnsConflict), currentUser);
        await using var _ = db;

        var actingAsAdmin1 = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var firstDemote = await new ChangeUserRoleCommandHandler(db, actingAsAdmin1)
            .Handle(new ChangeUserRoleCommand(admin2.Id, "Member"), CancellationToken.None);
        Assert.True(firstDemote.IsSuccess);

        var actingAsAdmin2 = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin2.Id };
        var secondDemote = await new ChangeUserRoleCommandHandler(db, actingAsAdmin2)
            .Handle(new ChangeUserRoleCommand(admin1.Id, "Member"), CancellationToken.None);

        Assert.False(secondDemote.IsSuccess);
        Assert.Equal(ErrorType.Conflict, secondDemote.Error!.Type);
    }

    [Fact]
    public async Task Handle_PromoteMemberToAdmin_ReusesExistingAdminRole()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, adminRole, admin1, _, member) = await SeedAsync(nameof(Handle_PromoteMemberToAdmin_ReusesExistingAdminRole), currentUser);
        await using var _ = db;

        var actingUser = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var result = await new ChangeUserRoleCommandHandler(db, actingUser)
            .Handle(new ChangeUserRoleCommand(member.Id, "Admin"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = db.Users.Single(u => u.Id == member.Id);
        Assert.Equal(adminRole.Id, reloaded.RoleId);
        // The existing "Admin" role is reused, not duplicated - only one should ever exist per org.
        Assert.Equal(1, db.Roles.Count(r => r.Name == "Admin"));
    }

    [Fact]
    public async Task Handle_SameRoleRequested_IsNoOpSuccess()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 9 };
        var (db, _, admin1, _, member) = await SeedAsync(nameof(Handle_SameRoleRequested_IsNoOpSuccess), currentUser);
        await using var _ = db;

        var actingUser = new FakeCurrentUserContext { OrganizationId = 9, UserId = admin1.Id };
        var result = await new ChangeUserRoleCommandHandler(db, actingUser)
            .Handle(new ChangeUserRoleCommand(member.Id, "Member"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
