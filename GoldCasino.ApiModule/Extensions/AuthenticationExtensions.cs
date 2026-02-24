using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Helpers;
using GoldCasino.ApiModule.Services;
using Microsoft.Extensions.Configuration;

namespace GoldCasino.ApiModule.Extensions;

public static class AuthenticationExtensions
{
	public static IServiceCollection AddEncryptedCookieAuth(this IServiceCollection services, IConfiguration cfg)
	{
		// Bind options so handler can read them
		services.AddOptions<CookieAuthOptions>()
				.Bind(cfg.GetSection("CookieAuth"))
				.ValidateDataAnnotations()
				.ValidateOnStart();

		// Register crypto helper (adjust if you already have it elsewhere)
		services.AddSingleton<CookieEncryptionHelper>();

		services.AddAuthentication() // no defaults here; host controls defaults
					.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, EncryptedCookieHandler>(
							AuthDefaults.EncryptedCookieScheme,
							options => { /* nothing to configure here; handler reads CookieAuthOptions */ });

		// HttpContext accessor is required for cookie service
		services.AddHttpContextAccessor();
		services.AddSingleton<IAuthCookieService, AuthCookieService>();

		return services;
	}
}
