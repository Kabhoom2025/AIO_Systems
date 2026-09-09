using System.Security.Claims;
using FluentValidation;
using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/risk-rules")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class RiskRulesController : ControllerBase
{
    private readonly IRiskRuleManagementService _riskRuleService;
    private readonly IValidator<CreateRiskRuleRequest> _createValidator;
    private readonly IValidator<UpdateRiskRuleRequest> _updateValidator;

    public RiskRulesController(
        IRiskRuleManagementService riskRuleService,
        IValidator<CreateRiskRuleRequest> createValidator,
        IValidator<UpdateRiskRuleRequest> updateValidator)
    {
        _riskRuleService = riskRuleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RiskRuleDto>>> GetAll(CancellationToken ct) =>
        Ok(await _riskRuleService.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<RiskRuleDto>> Create(CreateRiskRuleRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);
        var result = await _riskRuleService.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RiskRuleDto>> Update(Guid id, UpdateRiskRuleRequest request, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAsync(request, ct);
        return Ok(await _riskRuleService.UpdateAsync(id, request, GetUserId(), ct));
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
