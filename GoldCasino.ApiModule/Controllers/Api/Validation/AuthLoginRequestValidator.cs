using GoldCasino.ApiModule.Models.Auth;

namespace GoldCasino.ApiModule.Controllers.Api.Validation;

public class AuthLoginRequestValidator : AbstractValidator<AuthLoginRequest>
{
    public AuthLoginRequestValidator()
    { 
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(5).WithMessage("Language must be 2-5 chars (e.g., 'en' or 'en-US').");
    }
}
