using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.Common.Validation;

namespace GoldCasino.ApiModule.Services.BusinessApi.Validation;

[ServiceValidator]
public class EntityAddValidator : AbstractValidator<EntityAdd>
{
    public EntityAddValidator()
    {
        RuleFor(x => x.EmployeeEntityId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.EntityAdd_EmployeeEntityIdInvalid);

        RuleFor(x => x.CategoryId)
            .GreaterThanOrEqualTo(0).WithMessage("CategoryID must be >= 0");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(4).WithMessage("Password must be at least 4 characters")
            .MaximumLength(20).WithMessage("Password must be at most 20 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Mobile)
            .NotEmpty();

        RuleFor(x => x.CountryISO)
            .NotEmpty()
            .Length(2, 3).WithMessage("CountryISO must be ISO alpha-2 or alpha-3 code");

        RuleFor(x => x.AffiliateEntityId)
            .GreaterThanOrEqualTo(0);
    }
}
