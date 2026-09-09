using System.Security.Cryptography;
using FluentValidation;
using FoodOrder.Application.DTOs.Auth;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Infrastructure.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
#pragma warning disable IDE0060

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequestDto> _validator;
    private readonly SsoService _sso;
    private readonly IConfiguration _config;

    public AuthController(
        IAuthService authService,
        IValidator<LoginRequestDto> validator,
        SsoService sso,
        IConfiguration config)
    {
        _authService = authService;
        _validator   = validator;
        _sso         = sso;
        _config      = config;
    }

    /// <summary>Sends a password-reset email if the address matches a known account.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var frontendUrl = _config["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var message = await _authService.ForgotPasswordAsync(request.Email, frontendUrl);
        return Ok(ApiResponse<string>.SuccessResult(message, message));
    }

    /// <summary>Resets a user's password using a valid reset token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<string>.SuccessResult("Password reset successfully. You can now sign in.", "Password reset successfully."));
    }

    /// <summary>Authenticates a user and returns a JWT bearer token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var error = await ValidateAsync(_validator, request);
        if (error != null) return error;

        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResult(result, "Login successful."));
    }

    // ── SSO — initiate ────────────────────────────────────────────────────────

    /// <summary>Initiates OAuth2 authorization code flow for the given provider.</summary>
    [HttpGet("sso/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> SsoInitiate(string provider)
    {
        if (!await _sso.IsProviderConfiguredAsync(provider))
            return Redirect(FrontendError($"{provider} SSO is not configured. Add credentials in Settings."));

        var state       = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
        var callbackUrl = BuildCallbackUrl(provider);

        Response.Cookies.Append($"oauth_state_{provider}", state, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            MaxAge   = TimeSpan.FromMinutes(10),
            Secure   = Request.IsHttps,
        });

        var authUrl = await _sso.BuildAuthorizationUrlAsync(provider, callbackUrl, state);
        return Redirect(authUrl);
    }

    // ── SSO — callback ────────────────────────────────────────────────────────

    /// <summary>Handles the OAuth2 callback from the identity provider.</summary>
    [HttpGet("sso/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> SsoCallback(string provider, [FromQuery] string? code,
        [FromQuery] string? state, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
            return Redirect(FrontendError($"SSO cancelled or denied: {error}"));

        if (string.IsNullOrEmpty(code))
            return Redirect(FrontendError("Authorization code missing."));

        // Verify state (CSRF protection)
        var cookieKey   = $"oauth_state_{provider}";
        var storedState = Request.Cookies[cookieKey];
        Response.Cookies.Delete(cookieKey);
        if (string.IsNullOrEmpty(storedState) || storedState != state)
            return Redirect(FrontendError("Invalid SSO state. Please try again."));

        try
        {
            var callbackUrl    = BuildCallbackUrl(provider);
            var (email, name)  = await _sso.ExchangeCodeAsync(provider, code, callbackUrl);

            if (string.IsNullOrWhiteSpace(email))
                return Redirect(FrontendError($"Could not retrieve email from {provider}. Make sure your email is public."));

            var loginResult = await _authService.SsoLoginAsync(email, provider);

            var frontendUrl = _config["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var query = $"token={Uri.EscapeDataString(loginResult.Token)}" +
                        $"&name={Uri.EscapeDataString(loginResult.Name)}" +
                        $"&role={Uri.EscapeDataString(loginResult.RoleName)}";
            return Redirect($"{frontendUrl}/auth/sso-callback?{query}");
        }
        catch (Exception ex)
        {
            return Redirect(FrontendError(ex.Message));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string BuildCallbackUrl(string provider)
    {
        var scheme = Request.Scheme;
        var host   = Request.Host.Value;
        return $"{scheme}://{host}/api/auth/sso/{provider}/callback";
    }

    private string FrontendError(string message)
    {
        var frontendUrl = _config["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        return $"{frontendUrl}/auth/login?sso_error={Uri.EscapeDataString(message)}";
    }

}
