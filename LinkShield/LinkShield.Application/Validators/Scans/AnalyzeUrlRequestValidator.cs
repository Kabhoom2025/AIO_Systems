using FluentValidation;
using LinkShield.Application.DTOs.Scans;

namespace LinkShield.Application.Validators.Scans;

public class AnalyzeUrlRequestValidator : AbstractValidator<AnalyzeUrlRequestDto>
{
    private static readonly string[] AllowedSchemes = ["http", "https"];

    public AnalyzeUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeAWellFormedHttpUrl)
            .WithMessage("Url must be a well-formed absolute http or https URL.");
    }

    private static bool BeAWellFormedHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
}
