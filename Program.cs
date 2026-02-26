using BusinessApi;
using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Infrastructure;
using GoldCasino.ApiModule.Middlewares;
using IPinfo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartWinners.Configuration;
using SmartWinners.Extensions;
using SmartWinners.Helpers;
using SmartWinners.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var config = builder.Configuration;

services.Configure<IpInfoOptions>(config.GetSection(IpInfoOptions.SectionName));
services.Configure<LadderBonusOptions>(config.GetSection(LadderBonusOptions.SectionName));
services.AddSingleton(provider =>
{
	var options = provider.GetRequiredService<IOptions<IpInfoOptions>>().Value;
	if (string.IsNullOrWhiteSpace(options.Token))
	{
		throw new InvalidOperationException("IpInfo token is not configured.");
	}

	return new IPinfoClient.Builder()
		.AccessToken(options.Token)
		.Build();
});
services.AddSingleton<IIpInfoCountryResolver, IpInfoCountryResolver>();

services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		builder =>
		{
			builder.AllowAnyOrigin()
						 .AllowAnyHeader()
						 .AllowAnyMethod();
		});
});

services.AddApiModule(config);
services.AddControllers(options => options.Filters.Add<UpstreamExceptionFilter>())
				.AddApiModuleControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});

services.AddScoped<BusinessApiService>();
services.AddScoped<BusinessAPIClient>();
services.AddScoped<StripeService>();
services.AddScoped<PlisioService>();
services.AddScoped<IdentityService>();
services.AddScoped<StudiosService>();
services.AddScoped<LadderBonusService>();

// Payment success handlers for post-payment processing (bonuses, vouchers, etc.)
services.AddScoped<SmartWinners.Services.Payment.PaymentSuccessOrchestrator>();
services.AddScoped<SmartWinners.Services.Payment.IPaymentSuccessHandler, SmartWinners.Services.Payment.Handlers.WelcomeBonusHandler>();
services.AddScoped<SmartWinners.Services.Payment.IPaymentSuccessHandler, SmartWinners.Services.Payment.Handlers.WelcomeLadderBonusHandler>();
services.AddScoped<SmartWinners.Services.Payment.IPaymentSuccessHandler, SmartWinners.Services.Payment.Handlers.DepositBonusHandler>();

services.AddScoped<IGoldslotService, GoldslotService>();
services.AddScoped<GamesService>();
services.AddScoped<GameCategoriesService>();
services.AddScoped<LotteryService>();
services.AddScoped<TopWinnersService>();
services.AddScoped<ILanguageSyncService, LanguageSyncService>();
services.AddMemoryCache();
services.AddHostedService<SupportedLanguagesSyncHostedService>();

services.AddHttpClient();
services.AddSingleton<ITranslator, OpenAiGpt52Translator>();


services.AddAuthentication(options =>
{
	options.DefaultScheme = "Combined";
	options.DefaultAuthenticateScheme = "Combined";
	options.DefaultChallengeScheme = "Combined";
	options.DefaultSignInScheme = "Combined";
})
.AddPolicyScheme("Combined", "Combined", o =>
{
	o.ForwardDefaultSelector = ctx =>
	{
		var p = ctx.Request.Path;
		if (p.StartsWithSegments("/umbraco") || p.StartsWithSegments("/install"))
			return Constants.Security.BackOfficeAuthenticationType;
		return AuthDefaults.EncryptedCookieScheme;
	};
});

services.AddSwaggerGen();

builder.CreateUmbracoBuilder()
		.AddBackOffice()
		.AddWebsite()
		.AddComposers()
		.Build();

builder.Services.PostConfigureAll<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(o =>
{
	o.DefaultScheme = "Combined";
	o.DefaultAuthenticateScheme = "Combined";
	o.DefaultChallengeScheme = "Combined";
	o.DefaultSignInScheme = "Combined";
});

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
EnvironmentHelper.HttpContextAccessor = httpContextAccessor;
EnvironmentHelper.Environment = app.Environment;

app.UseCors("AllowAll");
app.UseSession();
// app.UseMiddleware<IpMiddleware>();
app.UseMiddleware<LogContextMiddleware>();

app.Use(async (context, next) =>
{
	if (context.Request.Query.TryGetValue("r", out var redirect))
		context.Response.Headers["Redirect"] = redirect.ToString();

	await next();
});

app.Use(async (context, next) =>
{
	var host = context.Request.Host.Host.ToLower();

	// Check if the request is coming from one of the beta domains
	if (host == "beta7.player1.win" ||
							host == "beta2.playerclub.app")
	{
		context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
	}

	await next();
});

app.Use(async (context, next) =>
{
	if (context.Request.Host.Value.Contains("beta") &&
							context.Request.Path.Value.Contains("robots.txt"))
	{
		context.Response.StatusCode = 404;
		return;
	}

	//if (!context.Request.Host.Value.StartsWith("www", StringComparison.OrdinalIgnoreCase) &&
	//						!context.Request.Host.Value.Contains("beta") && !context.Request.Host.Value.Contains("192") &&
	//						!context.Request.Host.Value.Equals("89.23.5.24"))
	//{
	//	var newUrl =
	//							$"{context.Request.Scheme}://www.{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
	//	context.Response.Redirect(newUrl, permanent: true);
	//	return;
	//}


	// Increase the maximum request body size to 100KB
	context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 100 * 1024 * 1024;
	await next.Invoke();
});

app.UseExceptionHandler("/Error/Handler");
app.UseStatusCodePagesWithReExecute("/Error/NotFound");
if (app.Environment.IsDevelopment())
{
	//app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		var context = ctx.Context;
		var seoConfig = EnvironmentHelper.SeoConfiguration;

		if (seoConfig is null)
		{
			ConfigReader.ReadFromJsonConfig<SeoConfiguration>(EnvironmentHelper.ConfigName,
										out seoConfig);
		}

		if (seoConfig?.CacheControlExpireTime == 0)
		{
			context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
		}
		else if (seoConfig.CacheControlPath.Any(x =>
													 context.Request.Path.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
		{
			var cacheControl =
										$"max-age={TimeSpan.FromMinutes(seoConfig.CacheControlExpireTime).TotalSeconds}, must-revalidate";
			context.Response.Headers.Add("Cache-Control", cacheControl);
		}

		if (ctx.Context.Request.Path.Value.Contains("appsettings.json",
											StringComparison.OrdinalIgnoreCase) ||
									ctx.Context.Request.Path.Value.Contains("apiconfig.json",
											StringComparison.OrdinalIgnoreCase) ||
									ctx.Context.Request.Path.Value.Contains("IpAddressesList.xml",
											StringComparison.OrdinalIgnoreCase) ||
									ctx.Context.Request.Path.Value.Contains("apiconfig-live.json",
											StringComparison.OrdinalIgnoreCase))
		{
			ctx.Context.Response.Redirect("/");
		}

		if (ctx.Context.Request.Path.Value.Contains("CoinbaseLog"))
		{
			ctx.Context.Response.Redirect("/");
		}
	}
});

// app.UseAuthentication();
// app.UseAuthorization();

app.UseUmbraco()
	.WithMiddleware(u =>
	{
		u.AppBuilder.UseRouting();
		u.AppBuilder.UseRequestLocalization(op =>
		{
			CultureInfo[] SupportedCultures =
			[
					new CultureInfo("en"),
					new CultureInfo("fr"),
					new CultureInfo("es"),
					new CultureInfo("ru"),
					new CultureInfo("uk"),
					new CultureInfo("he"),
					new CultureInfo("th"),
					new CultureInfo("pt"),
					new CultureInfo("vi"),
					new CultureInfo("ms"),
			];

			op.DefaultRequestCulture = new RequestCulture("en");
			op.SupportedCultures = SupportedCultures;
			op.SupportedUICultures = SupportedCultures;
			// The order here matters: custom provider first.
			op.RequestCultureProviders =
			[
				new UrlSegmentRequestCultureProvider(), //from url segment
				new QueryStringRequestCultureProvider(), // ?culture=fr
				new CookieRequestCultureProvider(){ CookieName = "Culture" }, // from a cookie
				new AcceptLanguageHeaderRequestCultureProvider(), // based on browser settings				
			];
		});
		u.AppBuilder.UseAuthentication();
		u.AppBuilder.UseMiddleware<LidCleanupMiddleware>();
		u.AppBuilder.UseAuthorization();
		u.UseBackOffice();
		u.UseWebsite();
	})
	.WithEndpoints(u =>
	{
		u.UseInstallerEndpoints();
		u.UseBackOfficeEndpoints();
		u.UseWebsiteEndpoints();
	});

await app.RunAsync();
