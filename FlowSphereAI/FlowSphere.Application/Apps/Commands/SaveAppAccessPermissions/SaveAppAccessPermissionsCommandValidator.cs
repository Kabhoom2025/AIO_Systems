using FluentValidation;

namespace FlowSphere.Application.Apps.Commands.SaveAppAccessPermissions;

public class SaveAppAccessPermissionsCommandValidator : AbstractValidator<SaveAppAccessPermissionsCommand>
{
    public SaveAppAccessPermissionsCommandValidator()
    {
        RuleFor(x => x.AppId).GreaterThan(0);
        RuleFor(x => x.AccessPermissionsJson).NotEmpty();
    }
}
