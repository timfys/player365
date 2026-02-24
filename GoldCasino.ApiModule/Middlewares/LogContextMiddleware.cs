using GoldCasino.ApiModule.Extensions;
using Serilog.Context;

namespace GoldCasino.ApiModule.Middlewares;

public class LogContextMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context)
	{
		var isAuthorized = context.User.Identity?.IsAuthenticated ?? false;
		string userInfo;
		if (!isAuthorized)
			userInfo = "Anonymous";
		else
		{
			var user = context.User.ToUserApiAccess();
			userInfo = $"EntityId: {user.EntityId}, Username: {user.Username}";
		}
		var ePath = $"{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
		using (LogContext.PushProperty("User", userInfo))
		using (LogContext.PushProperty("DomainIp", GetHostIp(context)))
		using (LogContext.PushProperty("Url", ePath))
		using (LogContext.PushProperty("IP", GetUserIp(context)))
		{
			await next.Invoke(context);
		}
	}

	private static string GetUserIp(HttpContext context)
	{
		if (context.Request.Host.Value.Contains("localhost"))
		{
			return context.Connection.RemoteIpAddress.ToString();
		}

		return context.Request.Headers["CF-Connecting-IP"];
	}

	private static string? GetHostIp(HttpContext context)
	{
		var addresses = context.Connection.LocalIpAddress?.ToString();
		return addresses;

	}
}
