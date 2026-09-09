using FluentValidation;

namespace FlowSphere.Application.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().Must(name => name is "Admin" or "Member")
            .WithMessage("Role must be either 'Admin' or 'Member'.");
    }
}
