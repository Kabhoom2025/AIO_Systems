using FlowSphere.Application.Auth.Commands.Register;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using FlowSphere.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FlowSphere.Tests.Application.Auth;

public class RegisterCommandHandlerTests
{
    private static RegisterCommandHandler CreateHandler(FlowSphere.Infrastructure.Data.FlowSphereDbContext db) =>
        new(
            db,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            Options.Create(new ClientSettings { BaseUrl = "http://test.local" }),
            NullLogger<RegisterCommandHandler>.Instance);

    [Fact]
    public async Task Handle_CreatesOrganizationRoleAndUser_Unverified()
    {
        var currentUser = new FakeCurrentUserContext();
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_CreatesOrganizationRoleAndUser_Unverified));
        var handler = CreateHandler(db);

        var result = await handler.Handle(
            new RegisterCommand("Acme Corp", "Alice Admin", "alice@acme.test", "Password123"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsEmailVerified);
        Assert.Contains("verify-email?token=", result.Value.VerificationLink);

        var user = db.Users.IgnoreQueryFilters().Single(u => u.Email == "alice@acme.test");
        Assert.False(user.IsEmailVerified);
        Assert.NotNull(user.EmailVerificationToken);
        Assert.NotNull(user.EmailVerificationTokenExpiresAt);

        var role = db.Roles.IgnoreQueryFilters().Single(r => r.Id == user.RoleId);
        Assert.Equal("Admin", role.Name);
        Assert.Contains(PermissionCatalog.UsersManage, role.PermissionList);
    }

    [Fact]
    public async Task Handle_RejectsDuplicateEmail_AcrossOrganizations()
    {
        const string dbName = nameof(Handle_RejectsDuplicateEmail_AcrossOrganizations);
        var currentUser = new FakeCurrentUserContext();

        await using (var db1 = TestDbContextFactory.Create(currentUser, dbName))
        {
            await CreateHandler(db1).Handle(
                new RegisterCommand("Acme Corp", "Alice Admin", "dupe@acme.test", "Password123"),
                CancellationToken.None);
        }

        await using var db2 = TestDbContextFactory.Create(currentUser, dbName);
        var result = await CreateHandler(db2).Handle(
            new RegisterCommand("Globex Inc", "Bob Admin", "dupe@acme.test", "Password123"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }
}
