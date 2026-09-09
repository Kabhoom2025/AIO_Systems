using System.Security.Claims;
using FluentValidation;
using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/brands")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class BrandsController : ControllerBase
{
    private readonly IBrandManagementService _brandService;
    private readonly IValidator<CreateBrandProfileRequest> _validator;

    public BrandsController(IBrandManagementService brandService, IValidator<CreateBrandProfileRequest> validator)
    {
        _brandService = brandService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BrandProfileDto>>> GetAll(CancellationToken ct) =>
        Ok(await _brandService.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<BrandProfileDto>> Create(CreateBrandProfileRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        var result = await _brandService.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
