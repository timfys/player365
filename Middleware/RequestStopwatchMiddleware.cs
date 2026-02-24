using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SmartWinners.Middleware;

public class RequestStopwatchMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {

        var watch = new Stopwatch();
        watch.Start();
        context.Items["RequestStopwatch"] = watch;

        return next(context);
    }
}
