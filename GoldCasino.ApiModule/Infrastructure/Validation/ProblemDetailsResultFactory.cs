using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace GoldCasino.ApiModule.Infrastructure.Validation
{
  public sealed class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
  {
    public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails? validationProblemDetails)
    {
      var errors = (validationProblemDetails?.Errors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase))
          .ToDictionary(
              kvp => string.IsNullOrWhiteSpace(kvp.Key) ? "$" : kvp.Key,
              kvp => kvp.Value.Select(msg => new
              {
                path = string.IsNullOrWhiteSpace(kvp.Key) ? "$" : kvp.Key,
                message = msg
              }).ToArray());

      var pd = new ProblemDetails
      {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation failed.",
        Type = "https://httpstatuses.com/400",
        Detail = "Request contains invalid fields."
      };
      pd.Extensions["code"] = "VALIDATION_ERROR";
      pd.Extensions["errors"] = errors;
      pd.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

      return new ObjectResult(pd) { StatusCode = StatusCodes.Status400BadRequest };
    }
  }
}