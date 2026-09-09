using AIO_Systems.DTOs.Auth;
using AIO_Systems.Services.Interfaces;
using AIO_Systems.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIO_Systems.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<object>.FailureResult("Email and password are required."));

        var result = await authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResult(result, "Login successful."));
    }
}
