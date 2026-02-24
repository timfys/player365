namespace GoldCasino.ApiModule.Services.Common.Validation;

public interface IValidationRunner
{
  /// <summary>
  /// Validates the model using all registered FluentValidation validators for T.
  /// Returns null when valid; otherwise an ApiError with the first failure code/message.
  /// </summary>
  Task<Error?> ValidateAsync<T>(T model) where T : class;
}
