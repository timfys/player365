using GoldCasino.ApiModule.Auth;

namespace GoldCasino.ApiModule.Middlewares;

public class LidCleanupMiddleware(RequestDelegate next)
{
	private readonly RequestDelegate _next = next;

	public async Task Invoke(HttpContext context)
	{
		if (TryGetRedirectTarget(context, out var target))
		{
			context.Items.Remove(LidAuthenticationContext.RedirectUrlItemKey);
			context.Response.Redirect(target, permanent: false);
			return;
		}

		await _next(context);
	}

	private static bool TryGetRedirectTarget(HttpContext context, out string? target)
	{
		target = null;
		if (!context.Items.TryGetValue(LidAuthenticationContext.RedirectUrlItemKey, out var stored) ||
			stored is not string url ||
			string.IsNullOrWhiteSpace(url))
		{
			return false;
		}

		if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
			return false;

		if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
			return false;

		if (context.Response.HasStarted)
			return false;

		target = url;
		return true;
	}
}
