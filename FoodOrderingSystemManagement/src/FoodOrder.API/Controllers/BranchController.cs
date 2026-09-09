using FoodOrder.Application.DTOs.Branch;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/branches")]
[Authorize]
public class BranchController(IBranchService branchService) : BaseApiController
{
    /// <summary>Lists branches for the caller's organization (SuperAdmin may pass ?organizationId=).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int? organizationId)
    {
        var branches = await branchService.GetMineAsync(organizationId);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.SuccessResult(branches));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
    {
        var branch = await branchService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<BranchDto>.SuccessResult(branch, "Branch created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
    {
        var branch = await branchService.UpdateAsync(id, dto);
        return Ok(ApiResponse<BranchDto>.SuccessResult(branch, "Branch updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await branchService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Branch deleted successfully."));
    }

    /// <summary>
    /// Per-branch orders/revenue breakdown for the caller's organization — "compare my
    /// branches" (SuperAdmin may pass ?organizationId= to inspect any org).
    /// </summary>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports([FromQuery] int? organizationId)
    {
        var reports = await branchService.GetBranchReportsAsync(organizationId);
        return Ok(ApiResponse<List<BranchReportDto>>.SuccessResult(reports));
    }
}
