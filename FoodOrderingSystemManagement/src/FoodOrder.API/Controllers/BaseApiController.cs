using System.Security.Claims;
using FluentValidation;
using FoodOrder.Shared.Exceptions;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

/// <summary>
/// Base controller inherited by all API controllers.
/// Centralises repeated patterns — validation, user identity extraction — so
/// each controller only contains its own business-specific logic.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Runs the validator and returns a formatted 400 Bad Request if validation fails.
    /// Returns null when the model is valid.
    /// Usage: var error = await ValidateAsync(_validator, dto); if (error != null) return error;
    /// </summary>
    protected async Task<IActionResult?> ValidateAsync<T>(IValidator<T> validator, T model)
    {
        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<object>.FailureResult(errors));
        }
        return null;
    }

    /// <summary>
    /// Extracts the authenticated user's ID from the JWT NameIdentifier claim.
    /// Throws AppException (401) if the claim is missing or cannot be parsed.
    /// </summary>
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId))
            throw new UnauthorizedException("Invalid authentication token. Please log in again.");
        return userId;
    }

    /// <summary>Returns the authenticated user's OrganizationId claim, or null for SuperAdmin.</summary>
    protected int? GetCurrentOrganizationId()
    {
        var claim = User.FindFirst("organizationId");
        return claim != null && int.TryParse(claim.Value, out var orgId) ? orgId : null;
    }

    /// <summary>Returns the authenticated user's BranchId claim, or null for an org-wide user (or SuperAdmin).</summary>
    protected int? GetCurrentBranchId()
    {
        var claim = User.FindFirst("branchId");
        return claim != null && int.TryParse(claim.Value, out var branchId) ? branchId : null;
    }

    /// <summary>Returns the authenticated user's role name from the JWT Role claim.</summary>
    protected string GetCurrentUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    /// <summary>Returns the authenticated user's display name from the JWT Name claim.</summary>
    protected string GetCurrentUserName() =>
        User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}
