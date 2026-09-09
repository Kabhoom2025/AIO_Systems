using Chatbot.Application.Auth;
using Xunit;

namespace Chatbot.Tests.Application;

public class AuthValidatorsTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    [Theory]
    [InlineData("short1A", false)]
    [InlineData("nouppercase123", false)]
    [InlineData("NOLOWERCASE123", false)]
    [InlineData("NoDigitsHere", false)]
    [InlineData("ValidPass123", true)]
    public void RegisterRequestValidator_EnforcesPasswordComplexity(string password, bool expectedValid)
    {
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", password);

        var result = _registerValidator.Validate(request);

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_RejectsInvalidEmail()
    {
        var request = new RegisterRequest("Jane", "Doe", "not-an-email", "ValidPass123");

        var result = _registerValidator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginRequestValidator_RequiresEmailAndPassword()
    {
        var result = _loginValidator.Validate(new LoginRequest("", ""));

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
