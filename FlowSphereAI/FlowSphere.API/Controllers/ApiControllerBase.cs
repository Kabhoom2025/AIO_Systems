using FlowSphere.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>Maps a Result/Result&lt;T&gt; failure to the matching HTTP status code -
    /// the seam between the MediatR pipeline's Result pattern and REST responses.</summary>
    protected IActionResult HandleFailure(Error error) => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Conflict(new { error.Code, error.Message }),
        ErrorType.Unauthorized => Unauthorized(new { error.Code, error.Message }),
        ErrorType.QuotaExceeded => StatusCode(StatusCodes.Status429TooManyRequests, new { error.Code, error.Message }),
        ErrorType.Validation => BadRequest(new { error.Code, error.Message, errors = error.ValidationErrors }),
        _ => StatusCode(500, new { error.Code, error.Message })
    };

    protected IActionResult FromResult<T>(Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (!result.IsSuccess)
        {
            return HandleFailure(result.Error!);
        }

        return onSuccess is not null ? onSuccess(result.Value!) : Ok(result.Value);
    }

    protected IActionResult FromResult(Result result)
    {
        return result.IsSuccess ? Ok() : HandleFailure(result.Error!);
    }
}
