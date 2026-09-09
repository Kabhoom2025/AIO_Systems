namespace AIO_Systems.DTOs.Auth;

/// <summary>
/// Internal result from ITokenService — carries both the signed JWT string
/// and its expiry so the auth service can include ExpiresAt in the response.
/// </summary>
public class TokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
