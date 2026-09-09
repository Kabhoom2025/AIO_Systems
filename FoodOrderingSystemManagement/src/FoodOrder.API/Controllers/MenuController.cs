using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

/// <summary>
/// Public menu endpoint — no authentication required.
/// Used by the customer-facing QR-code menu page.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class MenuController : ControllerBase
{
    private readonly IFoodItemService _foodItemService;
    private readonly ICategoryService _categoryService;
    private readonly ISettingsService _settingsService;

    public MenuController(
        IFoodItemService foodItemService,
        ICategoryService categoryService,
        ISettingsService settingsService)
    {
        _foodItemService = foodItemService;
        _categoryService = categoryService;
        _settingsService = settingsService;
    }

    /// <summary>
    /// Returns all active categories and their available food items for the customer menu.
    /// Anonymous — there are no tenant claims to scope by, so the caller must say which
    /// branch's menu it wants explicitly.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMenu([FromQuery] int branchId)
    {
        var items      = await _foodItemService.GetAllByBranchAsync(branchId);
        var categories = await _categoryService.GetAllByBranchAsync(branchId);
        var settings   = await _settingsService.GetSettingsAsync();

        var activeCategories = categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Select(c => new
            {
                c.Id,
                c.CategoryName,
                Items = items
                    .Where(i => i.CategoryId == c.Id && i.IsAvailable)
                    .OrderBy(i => i.ItemName)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemName,
                        i.Price,
                        i.Image,
                        i.Description,
                        i.IsAvailable,
                    })
                    .ToList()
            })
            .Where(c => c.Items.Count > 0)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResult(new
        {
            restaurantName = settings?.RestaurantName ?? "Restaurant",
            categories     = activeCategories,
        }));
    }
}
