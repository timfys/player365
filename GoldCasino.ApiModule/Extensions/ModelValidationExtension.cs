using System.Reflection;
using GoldCasino.ApiModule.Infrastructure.Validation;
using GoldCasino.ApiModule.Services.Common.Validation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace GoldCasino.ApiModule.Extensions;

public static class ModelValidationExtension
{
  public static void AddValidation(this IServiceCollection services)
  {
    services.AddFluentValidationAutoValidation(options =>
    {
      options.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
    });

    // FluentValidation DI registration (scan this assembly once; auto-discovers all validators)
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Scoped);
    services.AddScoped<IValidationRunner, ValidationRunner>();
  }
}