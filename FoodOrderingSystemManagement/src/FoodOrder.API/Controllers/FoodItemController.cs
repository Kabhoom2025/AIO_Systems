using FluentValidation;
using FoodOrder.Application.DTOs.FoodItem;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class FoodItemController : BaseApiController
{
    private readonly IFoodItemService _foodItemService;
    private readonly IValidator<CreateFoodItemDto> _createValidator;
    private readonly IValidator<UpdateFoodItemDto> _updateValidator;

    public FoodItemController(
        IFoodItemService foodItemService,
        IValidator<CreateFoodItemDto> createValidator,
        IValidator<UpdateFoodItemDto> updateValidator)
    {
        _foodItemService = foodItemService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Get all food items — needed by POS and Waiter panel to display the menu.</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _foodItemService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<FoodItemDto>>.SuccessResult(items));
    }

    /// <summary>Get available food items for a specific category (Admin + Cashier).</summary>
    [HttpGet("by-category/{categoryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var items = await _foodItemService.GetByCategoryAsync(categoryId);
        return Ok(ApiResponse<IReadOnlyList<FoodItemDto>>.SuccessResult(items));
    }

    /// <summary>Get a single food item by ID (Admin + Cashier).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _foodItemService.GetByIdAsync(id);
        return Ok(ApiResponse<FoodItemDto>.SuccessResult(item));
    }

    /// <summary>Look up a food item by barcode — used by the POS scanner.</summary>
    [HttpGet("barcode/{code}")]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBarcode(string code)
    {
        var item = await _foodItemService.GetByBarcodeAsync(code);
        if (item is null)
            return NotFound(ApiResponse<object>.FailureResult($"No food item found for barcode '{code}'."));
        return Ok(ApiResponse<FoodItemDto>.SuccessResult(item));
    }

    /// <summary>Create a new food item (Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFoodItemDto dto)
    {
        var error = await ValidateAsync(_createValidator, dto);
        if (error != null) return error;

        var item = await _foodItemService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<FoodItemDto>.SuccessResult(item, "Food item created successfully."));
    }

    /// <summary>Update an existing food item (Admin only).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFoodItemDto dto)
    {
        var error = await ValidateAsync(_updateValidator, dto);
        if (error != null) return error;

        var item = await _foodItemService.UpdateAsync(id, dto);
        return Ok(ApiResponse<FoodItemDto>.SuccessResult(item, "Food item updated successfully."));
    }

    /// <summary>Soft-delete a food item by marking it unavailable (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _foodItemService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Food item deleted successfully."));
    }
}
