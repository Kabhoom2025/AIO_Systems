using FlowSphere.Infrastructure.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace FlowSphere.Tests.Infrastructure.Authentication;

public class CaptchaServiceTests
{
    private static CaptchaService CreateService() => new(DataProtectionProvider.Create("FlowSphere.Tests"));

    [Fact]
    public void Verify_CorrectAnswer_ReturnsTrue()
    {
        var service = CreateService();
        var challenge = service.GenerateChallenge();

        Assert.True(service.Verify(challenge.Token, challenge.A + challenge.B));
    }

    [Fact]
    public void Verify_WrongAnswer_ReturnsFalse()
    {
        var service = CreateService();
        var challenge = service.GenerateChallenge();

        Assert.False(service.Verify(challenge.Token, challenge.A + challenge.B + 1));
    }

    [Fact]
    public void Verify_TamperedToken_ReturnsFalse()
    {
        var service = CreateService();
        var challenge = service.GenerateChallenge();

        Assert.False(service.Verify(challenge.Token + "x", challenge.A + challenge.B));
    }

    [Fact]
    public void Verify_TokenFromDifferentProtectorInstance_StillVerifiesWithSamePurpose()
    {
        var provider = DataProtectionProvider.Create("FlowSphere.Tests.Shared");
        var service1 = new CaptchaService(provider);
        var service2 = new CaptchaService(provider);

        var challenge = service1.GenerateChallenge();

        Assert.True(service2.Verify(challenge.Token, challenge.A + challenge.B));
    }
}
