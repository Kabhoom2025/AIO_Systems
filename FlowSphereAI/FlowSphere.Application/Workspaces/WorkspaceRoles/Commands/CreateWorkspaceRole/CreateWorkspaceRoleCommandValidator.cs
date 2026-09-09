using FluentValidation;
using FlowSphere.Domain.Common;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.CreateWorkspaceRole;

public class CreateWorkspaceRoleCommandValidator : AbstractValidator<CreateWorkspaceRoleCommand>
{
    public CreateWorkspaceRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Permissions)
            .Must(p => p.Count > 0).WithMessage("Select at least one permission.")
            .Must(p => p.All(PermissionCatalog.WorkspaceScoped.Contains))
            .WithMessage("One or more permissions are not valid for a workspace role.");
    }
}
