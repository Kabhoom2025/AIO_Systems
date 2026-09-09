using System.Security.Cryptography;
using System.Text;

namespace LinkShield.Infrastructure.ApiKeyAuth;

/// <summary>
/// API keys are high-entropy random tokens, not human passwords — a fast deterministic hash
/// (SHA-256) is the standard approach for them (same as GitHub PATs), unlike password storage
/// which needs a slow hash (see Authentication/PasswordHasher.cs for that one). Deterministic
/// hashing is required here anyway, since lookup is "find the row whose hash matches", not
/// "verify against one known user's stored hash".
/// </summary>
public static class ApiKeyHasher
{
    private const string Prefix = "ls_live_";

    public static (string RawKey, string KeyPrefix, string KeyHash) GenerateNewKey()
    {
        var randomPart = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var rawKey = Prefix + randomPart;
        var keyPrefix = rawKey[..Math.Min(rawKey.Length, 16)];
        return (rawKey, keyPrefix, Hash(rawKey));
    }

    public static string Hash(string rawKey) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
}
