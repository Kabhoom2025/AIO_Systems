using FluentValidation;

namespace FlowSphere.Application.Connectors.Commands.SetConnectorCredential;

public class SetConnectorCredentialCommandValidator : AbstractValidator<SetConnectorCredentialCommand>
{
    public SetConnectorCredentialCommandValidator()
    {
        RuleFor(x => x.ConnectorId).GreaterThan(0);
        RuleFor(x => x.KeyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).NotEmpty();
    }
}
