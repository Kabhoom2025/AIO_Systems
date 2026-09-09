using FluentValidation;
using LinkShield.Application.DTOs.ApiClients;
using LinkShield.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkShield.API.Controllers;

[ApiController]
[Route("api/v1/api-clients")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ApiClientsController : ControllerBase
{
    private readonly IApiClientService _apiClientService;
    private readonly IValidator<CreateApiClientRequest> _createClientValidator;
    private readonly IValidator<CreateApiKeyRequest> _createKeyValidator;

    public ApiClientsController(
        IApiClientService apiClientService,
        IValidator<CreateApiClientRequest> createClientValidator,
        IValidator<CreateApiKeyRequest> createKeyValidator)
    {
        _apiClientService = apiClientService;
        _createClientValidator = createClientValidator;
        _createKeyValidator = createKeyValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApiClientDto>>> GetAll(CancellationToken ct) =>
        Ok(await _apiClientService.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ApiClientDto>> Create(CreateApiClientRequest request, CancellationToken ct)
    {
        await _createClientValidator.ValidateAndThrowAsync(request, ct);
        var result = await _apiClientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>Returns the raw key exactly once — it is never recoverable after this response.</summary>
    [HttpPost("{clientId:guid}/keys")]
    public async Task<ActionResult<CreatedApiKeyDto>> CreateKey(Guid clientId, CreateApiKeyRequest request, CancellationToken ct)
    {
        await _createKeyValidator.ValidateAndThrowAsync(request, ct);
        return Ok(await _apiClientService.CreateKeyAsync(clientId, request, ct));
    }

    [HttpDelete("keys/{keyId:guid}")]
    public async Task<IActionResult> RevokeKey(Guid keyId, CancellationToken ct)
    {
        var revoked = await _apiClientService.RevokeKeyAsync(keyId, ct);
        return revoked ? NoContent() : NotFound();
    }
}
