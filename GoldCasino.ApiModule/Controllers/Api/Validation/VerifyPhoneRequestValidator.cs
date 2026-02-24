using GoldCasino.ApiModule.Models.Auth;

namespace GoldCasino.ApiModule.Controllers.Api.Validation;

public class VerifyPhoneRequestValidator : AbstractValidator<VerifyPhoneRequest>
{
	public VerifyPhoneRequestValidator()
	{
		RuleFor(x => x.Phone)
			.NotEmpty().WithMessage("Phone is required.");

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Code is required.");
	}
}

