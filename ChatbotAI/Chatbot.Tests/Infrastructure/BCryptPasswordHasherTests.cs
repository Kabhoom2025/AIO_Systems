using Chatbot.Infrastructure.Authentication;
using Xunit;

namespace Chatbot.Tests.Infrastructure;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesAHashThatVerifiesAgainstTheOriginalPassword()
    {
        var hash = _sut.Hash("SuperSecret123");

        Assert.NotEqual("SuperSecret123", hash);
        Assert.True(_sut.Verify("SuperSecret123", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = _sut.Hash("SuperSecret123");

        Assert.False(_sut.Verify("WrongPassword", hash));
    }
}
