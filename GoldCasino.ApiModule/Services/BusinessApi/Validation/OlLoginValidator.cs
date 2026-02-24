using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.Common.Validation;

namespace GoldCasino.ApiModule.Services.BusinessApi.Validation;

[ServiceValidator]
public class OlLoginValidator : AbstractValidator<OlLogin>
{
    public OlLoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(5).WithMessage("Language must be 2-5 chars (e.g., 'en' or 'en-US').");

        RuleFor(x => x.IP)
            .Must(ip => string.IsNullOrWhiteSpace(ip) || System.Net.IPAddress.TryParse(ip, out _))
            .WithMessage("IP must be a valid IPv4 or IPv6 address if provided.");
    }
}
