using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.Common.Validation;

namespace GoldCasino.ApiModule.Services.BusinessApi.Validation;

[ServiceValidator]
public class EntityFindValidator : AbstractValidator<EntityFind>
{
  public EntityFindValidator()
  {
    // Basic bounds
    RuleFor(x => x.LimitFrom)
        .GreaterThanOrEqualTo(0)
        .When(x => x.LimitFrom.HasValue);

    RuleFor(x => x.LimitCount)
        .GreaterThan(0).WithMessage("LimitCount must be greater than 0 when provided.")
        .When(x => x.LimitCount.HasValue);

    // Fields not empty strings
    RuleForEach(x => x.Fields)
        .NotEmpty().WithMessage("Field names cannot be empty.");

    // Filter keys/values not empty
    RuleForEach(x => x.Filter)
        .Must(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
        .WithMessage("Filter keys and values must be non-empty.");

    // Business rule: if filtering by Mobile or Phone, must also filter by Country
    RuleFor(x => x.Filter)
      .Must(filter =>
      {
        bool hasPhoneOrMobile = false;
        bool hasCountry = false;

        foreach (var key in filter!.Keys)
        {
          if (!hasPhoneOrMobile &&
              (key.Equals("Mobile", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("Phone", StringComparison.OrdinalIgnoreCase)))
            hasPhoneOrMobile = true;

          if (!hasCountry && key.Equals("Country", StringComparison.OrdinalIgnoreCase))
            hasCountry = true;

          if (hasPhoneOrMobile && hasCountry) break;
        }

        return !hasPhoneOrMobile || hasCountry;
      })
      .WithMessage("If filtering by mobile or phone, also must be filtering by country")
  .WithErrorCode(ErrorCodes.ValidationMissingCountry)
      .OverridePropertyName("Filter.Country")
      .When(x => x.Filter != null && x.Filter.Count > 0);
  }
}
