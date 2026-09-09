using FluentValidation;
using LinkShield.Application.DTOs.ApiClients;

namespace LinkShield.Application.Validators.ApiClients;

public class CreateApiClientRequestValidator : AbstractValidator<CreateApiClientRequest>
{
    public CreateApiClientRequestValidator()
    {
        RuleFor(x => x.OrganizationName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.DailyQuota).GreaterThan(0).When(x => x.DailyQuota.HasValue);
        RuleFor(x => x.RequestsPerMinute).GreaterThan(0).When(x => x.RequestsPerMinute.HasValue);
    }
}

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
    }
}

public class ExternalUrlCheckRequestValidator : AbstractValidator<ExternalUrlCheckRequest>
{
    private static readonly string[] AllowedSchemes = ["http", "https"];

    public ExternalUrlCheckRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Url must be a well-formed absolute http or https URL.");
    }
}
