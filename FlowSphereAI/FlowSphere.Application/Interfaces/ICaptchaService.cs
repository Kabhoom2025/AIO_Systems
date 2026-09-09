namespace FlowSphere.Application.Interfaces;

public record CaptchaChallenge(int A, int B, string Token);

/// <summary>A self-hosted math-challenge captcha - no third-party key (reCAPTCHA/Turnstile)
/// needed. The token is a tamper-proof data-protected payload carrying {a, b, expiresAt}, so
/// verification needs no server-side storage/cleanup.</summary>
public interface ICaptchaService
{
    CaptchaChallenge GenerateChallenge();
    bool Verify(string token, int answer);
}
