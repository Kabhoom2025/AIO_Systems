using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AddOnController : BaseApiController
{
    private readonly IAddOnService _addOnService;

    public AddOnController(IAddOnService addOnService)
    {
        _addOnService = addOnService;
    }

    /// <summary>Get all add-ons.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AddOnDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var addOns = await _addOnService.GetAllAsync();
        return Ok(ApiResponse<IReadOnlyList<AddOnDTO>>.SuccessResult(addOns));
    }

    /// <summary>Get add-ons for a specific food item.</summary>
    [HttpGet("by-item/{foodItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AddOnDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByFoodItem(int foodItemId)
    {
        var addOns = await _addOnService.GetByFoodItemIdAsync(foodItemId);
        return Ok(ApiResponse<IReadOnlyList<AddOnDTO>>.SuccessResult(addOns));
    }

    /// <summary>Get a single add-on by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AddOnDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var addOn = await _addOnService.GetByIdAsync(id);
        return Ok(ApiResponse<AddOnDTO>.SuccessResult(addOn));
    }

    /// <summary>Create a new add-on (Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AddOnDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAddOnRequest dto)
    {
        var addOn = await _addOnService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = addOn.Id },
            ApiResponse<AddOnDTO>.SuccessResult(addOn, "Add-on created successfully."));
    }

    /// <summary>Update an existing add-on (Admin only).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AddOnDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAddOnRequest dto)
    {
        var addOn = await _addOnService.UpdateAsync(id, dto);
        return Ok(ApiResponse<AddOnDTO>.SuccessResult(addOn, "Add-on updated successfully."));
    }

    /// <summary>Delete an add-on (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _addOnService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Add-on deleted successfully."));
    }
}
