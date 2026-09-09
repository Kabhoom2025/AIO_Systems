using FlowSphere.Application.Users.Commands.ChangeUserRole;
using FlowSphere.Application.Users.Commands.ConfirmUserEmail;
using FlowSphere.Application.Users.Commands.InviteUser;
using FlowSphere.Application.Users.Commands.ResetUserPassword;
using FlowSphere.Application.Users.Commands.SetUserActive;
using FlowSphere.Application.Users.Queries.GetOrganizationUsers;
using FlowSphere.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSphere.API.Controllers;

[Authorize]
[Route("api/users")]
[Authorize(Policy = PermissionCatalog.UsersManage)]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOrganizationUsersQuery(), cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Invite(InviteUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return FromResult(result);
    }

    public record SetActiveRequest(bool IsActive);

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> SetActive(int id, SetActiveRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SetUserActiveCommand(id, request.IsActive), cancellationToken);
        return FromResult(result);
    }

    public record ChangeRoleRequest(string RoleName);

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ChangeUserRoleCommand(id, request.RoleName), cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:int}/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ConfirmUserEmailCommand(id), cancellationToken);
        return FromResult(result);
    }

    public record ResetPasswordRequest(string NewPassword);

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ResetUserPasswordCommand(id, request.NewPassword), cancellationToken);
        return FromResult(result);
    }
}
