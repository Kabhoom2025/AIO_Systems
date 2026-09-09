using FoodOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IRolePermissionService _service;

    public PermissionController(IRolePermissionService service)
    {
        _service = service;
    }

    /// <summary>Returns all feature keys assigned to a role.</summary>
    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        var features = await _service.GetByRoleIdAsync(roleId);
        return Ok(new { success = true, data = features });
    }

    /// <summary>Replaces all permissions for a role. Admin only.</summary>
    [HttpPut("role/{roleId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveForRole(int roleId, [FromBody] List<string> features)
    {
        await _service.SaveForRoleAsync(roleId, features ?? new List<string>());
        return Ok(new { success = true, message = "Permissions saved successfully." });
    }
}
