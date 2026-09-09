namespace FoodOrder.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed binding for the "JwtSettings" section in appsettings.json.
/// Injected into TokenService via IOptions&lt;JwtSettings&gt;.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480;
}
