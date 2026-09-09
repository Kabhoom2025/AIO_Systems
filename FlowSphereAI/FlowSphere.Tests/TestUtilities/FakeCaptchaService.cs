using FlowSphere.Application.Interfaces;

namespace FlowSphere.Tests.TestUtilities;

/// <summary>Deterministic fake - "valid-token" + answer 7 always verifies, anything else fails.
/// Never used outside the test project.</summary>
public class FakeCaptchaService : ICaptchaService
{
    public const string ValidToken = "valid-token";
    public const int ValidAnswer = 7;

    public CaptchaChallenge GenerateChallenge() => new(3, 4, ValidToken);

    public bool Verify(string token, int answer) => token == ValidToken && answer == ValidAnswer;
}
