using FoodOrder.Application.DTOs.Rating;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Exceptions;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
        => _ratingService = ratingService;

    /// <summary>Returns aggregated rating stats for every food item (called once on menu load).</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetAllStats()
    {
        var stats = await _ratingService.GetAllStatsAsync();
        return Ok(ApiResponse<List<RatingStatsDto>>.SuccessResult(stats));
    }

    /// <summary>Returns top recommended items — high-rated and/or frequently ordered.</summary>
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended()
    {
        var items = await _ratingService.GetRecommendedAsync();
        return Ok(ApiResponse<List<RecommendedItemDto>>.SuccessResult(items));
    }

    /// <summary>Submits a rating for a food item. One rating per session per item.</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitRatingRequest request)
    {
        try
        {
            var stats = await _ratingService.SubmitRatingAsync(request);
            return Ok(ApiResponse<RatingStatsDto>.SuccessResult(stats, "Thank you for your feedback!"));
        }
        catch (AppException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
    }
}
