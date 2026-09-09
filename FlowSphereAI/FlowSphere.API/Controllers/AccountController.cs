using FlowSphere.Application.Auth.Commands.ChangePassword;
using FlowSphere.Application.Auth.Commands.ResendVerificationEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/account")]
public class AccountController : ApiControllerBase
{
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ResendVerificationEmailCommand(), cancellationToken);
        return FromResult(result);
    }
}
