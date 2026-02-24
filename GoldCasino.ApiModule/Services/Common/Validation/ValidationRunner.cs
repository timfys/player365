namespace GoldCasino.ApiModule.Services.Common.Validation;

internal sealed class ValidationRunner(IServiceProvider sp) : IValidationRunner
{
  public async Task<Error?> ValidateAsync<T>(T model) where T : class
  {
    // Resolve validators specifically for T (0..N). If none, treat as valid.
    var validators = sp.GetServices<IValidator<T>>();
    if (validators is null)
      return null;

    // Fast-path: most types have a single validator
    if (validators is ICollection<IValidator<T>> coll && coll.Count == 1)
    {
      var single = coll.First();
      var result = await single.ValidateAsync(model);
      if (!result.IsValid)
      {
        var first = result.Errors.First();
        var code = string.IsNullOrWhiteSpace(first.ErrorCode)
            ? ErrorCodes.ValidationFailed
            : first.ErrorCode!;
        return new Error(code, first.ErrorMessage);
      }
      return null;
    }

    foreach (var validator in validators)
    {
      var result = await validator.ValidateAsync(model);
      if (!result.IsValid)
      {
        var first = result.Errors.First();
        var code = string.IsNullOrWhiteSpace(first.ErrorCode)
            ? ErrorCodes.ValidationFailed
            : first.ErrorCode!;
        return new Error(code, first.ErrorMessage);
      }
    }

    return null;
  }
}
