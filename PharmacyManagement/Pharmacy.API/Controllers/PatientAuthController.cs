using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/patient-auth")]
public class PatientAuthController : ControllerBase
{
    private readonly IPatientAuthService _service;

    public PatientAuthController(IPatientAuthService service) => _service = service;

    [AllowAnonymous]
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest(new { message = "Phone number is required." });

        var result = await _service.RequestOtpAsync(dto.Phone);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var result = await _service.VerifyOtpAsync(dto.Phone, dto.Code);
        return result == null
            ? BadRequest(new { message = "Invalid or expired OTP." })
            : Ok(result);
    }
}
