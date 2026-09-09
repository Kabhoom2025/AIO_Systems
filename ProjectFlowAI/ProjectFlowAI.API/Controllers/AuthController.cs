using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Auth;
using ProjectFlowAI.Application.Features.Invitations;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public record LoginRequest(string Email, string Password, string? TwoFactorCode);
    public record RefreshRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);
    public record VerifyEmailRequest(Guid UserId, string Token);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public record VerifyTwoFactorRequest(string Code);
    public record GoogleLoginRequest(string IdToken);
    public record MicrosoftLoginRequest(string AccessToken);
    public record AcceptInvitationRequest(string Email, string Token, string Password, string FirstName, string LastName);

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName));
        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password, ip, request.TwoFactorCode));
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip));
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken));
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        await _mediator.Send(new VerifyEmailCommand(request.UserId, request.Token));
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email));
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await _mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword));
        return NoContent();
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<ActionResult<string>> EnableTwoFactor()
    {
        var userId = GetUserId();
        var qrUri = await _mediator.Send(new EnableTwoFactorCommand(userId));
        return Ok(new { qrCodeUri = qrUri });
    }

    [HttpPost("2fa/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorRequest request)
    {
        await _mediator.Send(new VerifyTwoFactorCommand(GetUserId(), request.Code));
        return NoContent();
    }

    [HttpPost("oauth/google")]
    public async Task<ActionResult<AuthResultDto>> GoogleLogin(GoogleLoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new GoogleLoginCommand(request.IdToken, ip));
        return Ok(result);
    }

    [HttpPost("oauth/microsoft")]
    public async Task<ActionResult<AuthResultDto>> MicrosoftLogin(MicrosoftLoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new MicrosoftLoginCommand(request.AccessToken, ip));
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(GetUserId()));
        return Ok(result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Route("api/invitations")]
public class InvitationsAcceptController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsAcceptController(IMediator mediator) => _mediator = mediator;

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(AuthController.AcceptInvitationRequest request)
    {
        var result = await _mediator.Send(new AcceptInvitationCommand(
            request.Email, request.Token, request.Password, request.FirstName, request.LastName));
        return Ok(result);
    }
}
