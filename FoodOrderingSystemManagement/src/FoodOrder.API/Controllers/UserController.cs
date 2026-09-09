using FoodOrder.Application.DTOs.User;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.SuccessResult(users));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto, GetCurrentOrganizationId());
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<UserDto>.SuccessResult(user, "User created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(ApiResponse<UserDto>.SuccessResult(user, "User updated successfully."));
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _userService.ToggleActiveAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.SuccessResult(new { }, "User status updated."));
    }
}
