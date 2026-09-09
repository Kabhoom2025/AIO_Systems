using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;

    public AttachmentsController(IMediator mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = PermissionCatalog.TasksView)]
    public async Task<IActionResult> Download(Guid id)
    {
        var file = await _mediator.Send(new GetWorkItemAttachmentFileQuery(id));
        var contentType = _fileStorage.GetContentType(file.FileName);
        var stream = _fileStorage.OpenRead(file.FileUrl);
        return File(stream, contentType, file.FileName);
    }
}
