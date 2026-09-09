using FlowSphere.Application.Common;
using FlowSphere.Application.Users.Commands.ResetUserPassword;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Users;

public class ResetUserPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingUser_UpdatesPasswordHash()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 3 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExistingUser_UpdatesPasswordHash));

        var user = new User { OrganizationId = 3, Name = "Bob", Email = "bob@acme.test", PasswordHash = "old-hash", RoleId = 1, IsActive = true, IsEmailVerified = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db, new FakePasswordHasher());
        var result = await handler.Handle(new ResetUserPasswordCommand(user.Id, "NewPassword123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var reloaded = db.Users.Single(u => u.Id == user.Id);
        Assert.True(new FakePasswordHasher().Verify("NewPassword123", reloaded.PasswordHash));
    }

    [Fact]
    public async Task Handle_UnknownUser_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 3 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_UnknownUser_ReturnsNotFound));

        var handler = new ResetUserPasswordCommandHandler(db, new FakePasswordHasher());
        var result = await handler.Handle(new ResetUserPasswordCommand(999, "NewPassword123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
