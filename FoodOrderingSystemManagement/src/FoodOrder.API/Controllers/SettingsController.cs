using FluentValidation;
using FoodOrder.Application.DTOs.Settings;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : BaseApiController
{
    private readonly ISettingsService _settingsService;
    private readonly IValidator<UpdateSettingsDto> _updateValidator;

    public SettingsController(ISettingsService settingsService, IValidator<UpdateSettingsDto> updateValidator)
    {
        _settingsService = settingsService;
        _updateValidator = updateValidator;
    }

    /// <summary>Get the current restaurant settings (Admin only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(ApiResponse<SettingsDto>.SuccessResult(settings));
    }

    /// <summary>Update restaurant settings — tax percentage, name, address, phone, GST (Admin only).</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsDto dto)
    {
        var error = await ValidateAsync(_updateValidator, dto);
        if (error != null) return error;

        var settings = await _settingsService.UpdateSettingsAsync(dto);
        return Ok(ApiResponse<SettingsDto>.SuccessResult(settings, "Settings updated successfully."));
    }
}
