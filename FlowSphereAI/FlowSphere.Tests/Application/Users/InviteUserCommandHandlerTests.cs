using FlowSphere.Application.Common;
using FlowSphere.Application.Users.Commands.InviteUser;
using FlowSphere.Domain.Common;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FlowSphere.Tests.Application.Users;

public class InviteUserCommandHandlerTests
{
    private static InviteUserCommandHandler CreateHandler(FlowSphere.Application.Interfaces.IApplicationDbContext db, FakeCurrentUserContext currentUser) =>
        new(db, currentUser, new FakePasswordHasher(),
            Options.Create(new ClientSettings { BaseUrl = "http://test.local" }),
            NullLogger<InviteUserCommandHandler>.Instance);

    [Fact]
    public async Task Handle_NoRoleSpecified_CreatesPendingUnverifiedUserWithNoRole()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NoRoleSpecified_CreatesPendingUnverifiedUserWithNoRole));

        var handler = CreateHandler(db, currentUser);
        var result = await handler.Handle(new InviteUserCommand("Bob", "bob@acme.test", "Password123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.RoleName);
        Assert.False(result.Value!.IsEmailVerified);

        var user = db.Users.IgnoreQueryFilters().Single();
        Assert.False(user.IsEmailVerified);
        Assert.Null(user.RoleId);
        Assert.Equal(5, user.OrganizationId);
        Assert.NotNull(user.EmailVerificationToken);
    }

    [Fact]
    public async Task Handle_FirstInviteWithRole_CreatesMemberRoleAndVerifiedUser()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_FirstInviteWithRole_CreatesMemberRoleAndVerifiedUser));

        var handler = CreateHandler(db, currentUser);
        var result = await handler.Handle(new InviteUserCommand("Bob", "bob@acme.test", "Password123", RoleName: "Member"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Member", result.Value!.RoleName);

        var user = db.Users.IgnoreQueryFilters().Single();
        Assert.True(user.IsEmailVerified);
        Assert.Equal(5, user.OrganizationId);

        var role = db.Roles.IgnoreQueryFilters().Single();
        Assert.Equal("Member", role.Name);
        Assert.DoesNotContain(PermissionCatalog.UsersManage, role.PermissionList);
    }

    [Fact]
    public async Task Handle_SecondInviteWithRole_ReusesExistingMemberRole()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_SecondInviteWithRole_ReusesExistingMemberRole));
        var handler = CreateHandler(db, currentUser);

        await handler.Handle(new InviteUserCommand("Bob", "bob@acme.test", "Password123", RoleName: "Member"), CancellationToken.None);
        await handler.Handle(new InviteUserCommand("Carol", "carol@acme.test", "Password123", RoleName: "Member"), CancellationToken.None);

        Assert.Single(db.Roles.IgnoreQueryFilters().Where(r => r.Name == "Member"));
        Assert.Equal(2, db.Users.IgnoreQueryFilters().Count());
    }

    [Fact]
    public async Task Handle_InviteWithHrRole_CreatesHrRoleWithApproveOnlyPermissions()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_InviteWithHrRole_CreatesHrRoleWithApproveOnlyPermissions));

        var handler = CreateHandler(db, currentUser);
        var result = await handler.Handle(new InviteUserCommand("Harper", "harper@acme.test", "Password123", RoleName: "HR"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("HR", result.Value!.RoleName);

        var role = db.Roles.IgnoreQueryFilters().Single();
        Assert.Equal("HR", role.Name);
        Assert.Contains(PermissionCatalog.WorkflowsApprove, role.PermissionList);
        Assert.DoesNotContain(PermissionCatalog.WorkflowsExecute, role.PermissionList);
        Assert.DoesNotContain(PermissionCatalog.AppsWrite, role.PermissionList);
    }

    [Fact]
    public async Task Handle_InviteWithEmployeeRole_CreatesEmployeeRoleWithSubmitOnlyPermissions()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_InviteWithEmployeeRole_CreatesEmployeeRoleWithSubmitOnlyPermissions));

        var handler = CreateHandler(db, currentUser);
        var result = await handler.Handle(new InviteUserCommand("Jordan", "jordan@acme.test", "Password123", RoleName: "Employee"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Employee", result.Value!.RoleName);

        var role = db.Roles.IgnoreQueryFilters().Single();
        Assert.Equal("Employee", role.Name);
        Assert.Contains(PermissionCatalog.AppsSubmit, role.PermissionList);
        Assert.DoesNotContain(PermissionCatalog.AppsWrite, role.PermissionList);
        Assert.DoesNotContain(PermissionCatalog.WorkflowsApprove, role.PermissionList);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 5 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_DuplicateEmail_ReturnsConflict));
        var handler = CreateHandler(db, currentUser);

        await handler.Handle(new InviteUserCommand("Bob", "bob@acme.test", "Password123", RoleName: "Member"), CancellationToken.None);
        var result = await handler.Handle(new InviteUserCommand("Bobby", "bob@acme.test", "Password456", RoleName: "Member"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}
