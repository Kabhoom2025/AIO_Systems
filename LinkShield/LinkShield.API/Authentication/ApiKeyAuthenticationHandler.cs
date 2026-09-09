using System.Security.Claims;
using System.Text.Encodings.Web;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkShield.API.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-Key";
}

/// <summary>Authenticates enterprise API requests via the X-API-Key header (spec section 30),
/// separate from the JWT bearer scheme used everywhere else. See IApiKeyValidator for the
/// actual hash lookup / quota check — this handler is just the ASP.NET Core adapter around it.</summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator apiKeyValidator)
        : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var headerValues))
            return AuthenticateResult.Fail($"Missing {ApiKeyAuthenticationOptions.HeaderName} header.");

        var rawKey = headerValues.ToString();
        var result = await _apiKeyValidator.ValidateAndRecordUsageAsync(rawKey, Context.RequestAborted);

        if (!result.IsValid)
            return AuthenticateResult.Fail(result.DenyReason ?? "Invalid API key.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, result.ApiClientId!.Value.ToString()),
            new Claim("organizationName", result.OrganizationName ?? string.Empty)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
