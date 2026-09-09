using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Uses its own "documents.*" permissions (added to PermissionCatalog) rather than
/// reusing "settings.*" — uploading an arbitrary attachment to a Branch/User/etc. is a
/// distinct capability from editing organization settings, so a dedicated module is the
/// better semantic fit and lets roles grant one without the other.</summary>
[Route("api/documents")]
[Authorize]
public class DocumentController : ApiControllerBase
{
    private readonly IDocumentService _service;

    public DocumentController(IDocumentService service) => _service = service;

    [Authorize(Policy = "documents.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? entityType, [FromQuery] int? entityId) =>
        Ok(await _service.GetAllAsync(OrgId, entityType, entityId));

    [Authorize(Policy = "documents.create")]
    [HttpPost]
    [RequestSizeLimit(21_000_000)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? entityType, [FromForm] int? entityId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("A file is required.");

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadAsync(
            OrgId, UserId, stream, file.FileName, file.ContentType, file.Length, entityType, entityId);

        return CreatedAtAction(nameof(GetAll), result);
    }

    [Authorize(Policy = "documents.view")]
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var (stream, fileName, contentType) = await _service.DownloadAsync(OrgId, id);
        var effectiveContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        return File(stream, effectiveContentType, fileName);
    }

    [Authorize(Policy = "documents.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }
}
