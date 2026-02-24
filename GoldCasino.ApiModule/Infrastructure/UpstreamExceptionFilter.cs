using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GoldCasino.ApiModule.Infrastructure;

public class UpstreamExceptionFilter(ILogger<UpstreamExceptionFilter> logger, IAuthCookieService authCookie) : IAsyncExceptionFilter
{
    private readonly ILogger<UpstreamExceptionFilter> _logger = logger;
    private readonly IAuthCookieService _authCookie = authCookie;

    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is AuthenticationServiceException authEx)
        {
            _authCookie.Delete();

            var authProblem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Type = "https://httpstatuses.com/401",
                Detail = authEx.Message
            };
            authProblem.Extensions["code"] = ErrorCodes.AuthFailed;
            authProblem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Result = new ObjectResult(authProblem)
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                DeclaredType = typeof(ProblemDetails)
            };
            context.ExceptionHandled = true;

            _logger.LogWarning(authEx, "Authentication failure returned by upstream service");
            return Task.CompletedTask;
        }

        if (context.Exception is UpstreamServiceException)
        {
            // Do not leak internal error details
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Bad Gateway",
                Type = "https://httpstatuses.com/502",
                Detail = "Upstream service is unavailable. Please try again later."
            };
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status502BadGateway,
                DeclaredType = typeof(ProblemDetails)
            };
            context.ExceptionHandled = true;

            _logger.LogError(context.Exception, "Upstream service failure");
        }

        return Task.CompletedTask;
    }
}
