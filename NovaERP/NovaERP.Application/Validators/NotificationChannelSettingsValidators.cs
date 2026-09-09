using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class UpdateNotificationChannelSettingsDtoValidator : AbstractValidator<UpdateNotificationChannelSettingsDto>
{
    public UpdateNotificationChannelSettingsDtoValidator()
    {
        RuleFor(x => x.SmtpPort).InclusiveBetween(1, 65535).When(x => x.SmtpPort.HasValue);
        RuleFor(x => x.SmtpFromEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.SmtpFromEmail));
    }
}

public class TestSendNotificationDtoValidator : AbstractValidator<TestSendNotificationDto>
{
    private static readonly string[] ValidChannels = { "Email", "Sms", "Push" };

    public TestSendNotificationDtoValidator()
    {
        RuleFor(x => x.Channel).NotEmpty().Must(c => ValidChannels.Contains(c))
            .WithMessage("Channel must be one of: Email, Sms, Push.");
        RuleFor(x => x.To).NotEmpty();
        RuleFor(x => x.Message).NotEmpty();
    }
}
