using Moq;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.Auth;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class RegisterCommandHandlerTests
{
    private static (RegisterCommandHandler Handler, Infrastructure.Data.ProjectFlowDbContext Db) CreateHandler()
    {
        var db = InMemoryDbContextFactory.Create();

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns<string>(p => $"hashed:{p}");

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns("fake-access-token");
        var counter = 0;
        tokens.Setup(t => t.GenerateRefreshToken()).Returns(() => $"refresh-{counter++}");
        tokens.Setup(t => t.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash:{t}");

        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var email = new Mock<IEmailSender>();
        email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterCommandHandler(db, hasher.Object, tokens.Object, clock.Object, email.Object);
        return (handler, db);
    }

    [Fact]
    public async Task Register_With_New_Email_Succeeds_And_Creates_User()
    {
        var (handler, db) = CreateHandler();
        var command = new RegisterCommand("new.user@example.com", "Passw0rd!", "New", "User");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.User.Id);
        Assert.Equal("new.user@example.com", result.User.Email);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Register_With_Duplicate_Email_Throws_ConflictException()
    {
        var (handler, db) = CreateHandler();
        db.Users.Add(new User { Email = "existing@example.com", PasswordHash = "x", FirstName = "A", LastName = "B" });
        await db.SaveChangesAsync();

        var command = new RegisterCommand("existing@example.com", "Passw0rd!", "New", "User");

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }
}
