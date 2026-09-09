using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _service.LoginAsync(dto, ip, userAgent);
        return result == null
            ? Unauthorized(new { message = "Invalid credentials or account locked." })
            : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _service.RefreshTokenAsync(dto.RefreshToken);
        return result == null
            ? Unauthorized(new { message = "Refresh token is invalid or expired." })
            : Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        await _service.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _service.ChangePasswordAsync(UserId, dto);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        // In production the token is emailed; returned here because no mail server is configured.
        var token = await _service.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "If the email exists, a reset link has been generated.", token });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _service.ResetPasswordAsync(dto);
        return NoContent();
    }

    [Authorize(Policy = "audit-logs.view")]
    [HttpGet("login-history")]
    public async Task<IActionResult> LoginHistory([FromQuery] int take = 100) =>
        Ok(await _service.GetLoginHistoryAsync(OrgId, take));

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        UserId,
        Name  = UserName,
        OrgId,
        Roles = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
        Permissions = (User.FindFirst("permissions")?.Value ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
    });
}
