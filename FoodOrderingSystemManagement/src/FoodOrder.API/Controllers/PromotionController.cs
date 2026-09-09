using FoodOrder.Application.DTOs.Promotion;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PromotionController : BaseApiController
{
    private readonly IPromotionService _svc;
    public PromotionController(IPromotionService svc) { _svc = svc; }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse<IReadOnlyList<PromotionDto>>.SuccessResult(await _svc.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(ApiResponse<PromotionDto>.SuccessResult(await _svc.GetByIdAsync(id)));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive() =>
        Ok(ApiResponse<IReadOnlyList<PromotionDto>>.SuccessResult(await _svc.GetActiveAsync()));

    /// <summary>
    /// Public endpoint — no auth required. Used by dashboard slider. Anonymous, so
    /// there are no tenant claims to scope by — the caller must say which
    /// organization's promotions it wants explicitly.
    /// </summary>
    [HttpGet("public/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicActive([FromQuery] int organizationId) =>
        Ok(ApiResponse<IReadOnlyList<PromotionDto>>.SuccessResult(await _svc.GetPublicActiveAsync(organizationId)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
    {
        var result = await _svc.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<PromotionDto>.SuccessResult(result, "Promotion created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePromotionDto dto) =>
        Ok(ApiResponse<PromotionDto>.SuccessResult(await _svc.UpdateAsync(id, dto)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new {}, "Promotion deleted."));
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var isActive = await _svc.ToggleActiveAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { isActive }, isActive ? "Promotion activated." : "Promotion deactivated."));
    }

    /// <summary>Validate and calculate discount for a promo code at checkout.</summary>
    [HttpPost("apply")]
    [AllowAnonymous]
    public async Task<IActionResult> Apply([FromBody] ApplyPromoCodeDto dto) =>
        Ok(ApiResponse<ApplyPromotionResultDto>.SuccessResult(await _svc.ApplyCodeAsync(dto)));
}
