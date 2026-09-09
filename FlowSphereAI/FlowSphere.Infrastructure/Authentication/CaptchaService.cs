using FlowSphere.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace FlowSphere.Infrastructure.Authentication;

public class CaptchaService : ICaptchaService
{
    private const string ProtectorPurpose = "FlowSphere.AppCaptcha.v1";
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

    private readonly IDataProtector _protector;
    private readonly Random _random = new();

    public CaptchaService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public CaptchaChallenge GenerateChallenge()
    {
        var a = _random.Next(1, 10);
        var b = _random.Next(1, 10);
        var expiresAt = DateTime.UtcNow.Add(ChallengeLifetime);
        var token = _protector.Protect($"{a}:{b}:{expiresAt.Ticks}");

        return new CaptchaChallenge(a, b, token);
    }

    public bool Verify(string token, int answer)
    {
        try
        {
            var parts = _protector.Unprotect(token).Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            var a = int.Parse(parts[0]);
            var b = int.Parse(parts[1]);
            var expiresAt = new DateTime(long.Parse(parts[2]), DateTimeKind.Utc);

            return DateTime.UtcNow < expiresAt && a + b == answer;
        }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or FormatException or OverflowException)
        {
            return false;
        }
    }
}
