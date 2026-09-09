using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.DocPages;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Authorize]
public class DocPagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public DocPagesController(IMediator mediator, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record CreateDocPageRequest(Guid OrganizationId, Guid? ProjectId, DocScope Scope, DocCategory? Category,
        string Title, string Content, Guid? ParentPageId);
    public record UpdateDocPageRequest(string Title, string Content, Guid? ParentPageId, DocCategory? Category);
    public record AddCommentRequest(string Body);

    [HttpGet("api/doc-pages")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<IReadOnlyList<DocPageSummaryDto>>> List(
        [FromQuery] Guid organizationId, [FromQuery] Guid? projectId, [FromQuery] DocScope? scope,
        [FromQuery] DocCategory? category, [FromQuery] Guid? parentPageId)
    {
        var result = await _mediator.Send(new ListDocPagesQuery(organizationId, projectId, scope, category, parentPageId));
        return Ok(result);
    }

    [HttpGet("api/doc-pages/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<DocPageDetailDto>> Get(Guid id)
    {
        var result = await _mediator.Send(new GetDocPageQuery(id));
        return Ok(result);
    }

    [HttpPost("api/doc-pages")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    public async Task<ActionResult<DocPageDetailDto>> Create(CreateDocPageRequest request)
    {
        var result = await _mediator.Send(new CreateDocPageCommand(request.OrganizationId, request.ProjectId,
            request.Scope, request.Category, request.Title, request.Content, request.ParentPageId, CurrentUserId));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("api/doc-pages/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    public async Task<ActionResult<DocPageDetailDto>> Update(Guid id, UpdateDocPageRequest request)
    {
        var result = await _mediator.Send(new UpdateDocPageCommand(id, request.Title, request.Content, request.ParentPageId, request.Category, CurrentUserId));
        return Ok(result);
    }

    [HttpDelete("api/doc-pages/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDocPageCommand(id));
        return NoContent();
    }

    [HttpGet("api/doc-pages/{id:guid}/versions")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<IReadOnlyList<DocPageVersionSummaryDto>>> ListVersions(Guid id)
    {
        var result = await _mediator.Send(new ListDocPageVersionsQuery(id));
        return Ok(result);
    }

    [HttpGet("api/doc-pages/{id:guid}/versions/{versionId:guid}")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<DocPageVersionDetailDto>> GetVersion(Guid id, Guid versionId)
    {
        var result = await _mediator.Send(new GetDocPageVersionQuery(id, versionId));
        return Ok(result);
    }

    [HttpPost("api/doc-pages/{id:guid}/versions/{versionId:guid}/restore")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    public async Task<ActionResult<DocPageDetailDto>> RestoreVersion(Guid id, Guid versionId)
    {
        var result = await _mediator.Send(new RestoreDocPageVersionCommand(id, versionId, CurrentUserId));
        return Ok(result);
    }

    [HttpGet("api/doc-pages/{id:guid}/comments")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<IReadOnlyList<DocPageCommentDto>>> ListComments(Guid id)
    {
        var result = await _mediator.Send(new ListDocPageCommentsQuery(id));
        return Ok(result);
    }

    [HttpPost("api/doc-pages/{id:guid}/comments")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public async Task<ActionResult<DocPageCommentDto>> AddComment(Guid id, AddCommentRequest request)
    {
        var result = await _mediator.Send(new AddDocPageCommentCommand(id, CurrentUserId, request.Body));
        return Ok(result);
    }

    [HttpDelete("api/doc-page-comments/{id:guid}")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await _mediator.Send(new DeleteDocPageCommentCommand(id));
        return NoContent();
    }

    [HttpPost("api/doc-pages/{id:guid}/images")]
    [Authorize(Policy = PermissionCatalog.DocsManage)]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImageUploadResultDto>> UploadImage(Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty.");
        await _mediator.Send(new GetDocPageQuery(id)); // throws NotFoundException (-> 404) if the page doesn't exist

        await using var stream = file.OpenReadStream();
        var stored = await _fileStorage.SaveAsync(stream, file.FileName);
        return Ok(new ImageUploadResultDto($"/api/doc-pages/images/{stored.RelativePath}"));
    }

    [HttpGet("api/doc-pages/images/{fileName}")]
    [Authorize(Policy = PermissionCatalog.DocsView)]
    public IActionResult GetImage(string fileName)
    {
        var contentType = _fileStorage.GetContentType(fileName);
        var stream = _fileStorage.OpenRead(fileName);
        return File(stream, contentType);
    }
}
