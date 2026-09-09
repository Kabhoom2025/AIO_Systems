using Moq;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.Auth;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class LoginCommandHandlerTests
{
    private static (LoginCommandHandler Handler, Infrastructure.Data.ProjectFlowDbContext Db) CreateHandler()
    {
        var db = InMemoryDbContextFactory.Create();

        var hasher = new Mock<IPasswordHasher>();
        // Convention for these tests: hashed password is "hashed:<plain>" and verification just re-checks that shape.
        hasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((hash, plain) => hash == $"hashed:{plain}");

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns("fake-access-token");
        tokens.Setup(t => t.GenerateRefreshToken()).Returns("fake-refresh-token");
        tokens.Setup(t => t.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash:{t}");

        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var totp = new Mock<ITotpService>();

        var handler = new LoginCommandHandler(db, hasher.Object, tokens.Object, clock.Object, totp.Object);
        return (handler, db);
    }

    [Fact]
    public async Task Login_With_Correct_Credentials_Succeeds()
    {
        var (handler, db) = CreateHandler();
        db.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = "hashed:Passw0rd!",
            FirstName = "A",
            LastName = "B",
            Status = UserStatus.Active
        });
        await db.SaveChangesAsync();

        var result = await handler.Handle(new LoginCommand("user@example.com", "Passw0rd!", "127.0.0.1"), CancellationToken.None);

        Assert.Equal("user@example.com", result.User.Email);
        Assert.NotEmpty(result.AccessToken);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Throws_UnauthorizedDomainException()
    {
        var (handler, db) = CreateHandler();
        db.Users.Add(new User
        {
            Email = "user2@example.com",
            PasswordHash = "hashed:Correct1",
            FirstName = "A",
            LastName = "B",
            Status = UserStatus.Active
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedDomainException>(() =>
            handler.Handle(new LoginCommand("user2@example.com", "WrongPassword1", "127.0.0.1"), CancellationToken.None));
    }
}
