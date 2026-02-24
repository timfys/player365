using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SmartWinners.Extensions;

	public static class LocalizationExtensions
	{
		private static readonly CultureInfo[] SupportedCultures =
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

		public static IApplicationBuilder UseCustomRequestLocalization(this IApplicationBuilder app)
		{
			var localizationOptions = new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("en"),
				SupportedCultures = SupportedCultures,
				SupportedUICultures = SupportedCultures,
				// The order here matters: custom provider first.
				RequestCultureProviders =
				{
					new UrlSegmentRequestCultureProvider(), //from url segment
					new QueryStringRequestCultureProvider(), // ?culture=fr-FR
					new CookieRequestCultureProvider(){ CookieName = "Culture" }, // from a cookie
					new AcceptLanguageHeaderRequestCultureProvider(), // based on browser settings				
				}
			};

			return app.UseRequestLocalization(localizationOptions);
		}


	}

	public class UrlSegmentRequestCultureProvider : RequestCultureProvider
	{

		private readonly Dictionary<string, string> _cultureMap = new(StringComparer.OrdinalIgnoreCase)
		{
			["en"] = "en",
			["fr"] = "fr",
			["es"] = "es",
			["ru"] = "ru",
			["uk"] = "uk",
			["he"] = "he",
			["th"] = "th",
			["pt"] = "pt",
			["vi"] = "vi",
			["ms"] = "ms",

		};

		public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			ArgumentNullException.ThrowIfNull(httpContext);

			var path = httpContext.Request.Path.Value;
			if (string.IsNullOrEmpty(path))
				return Task.FromResult<ProviderCultureResult>(null);

			// Split the URL into segments.
			var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length > 0)
			{
				var potentialCulture = segments[0].ToLowerInvariant();
				if (_cultureMap.TryGetValue(potentialCulture, out var fullCulture))
				{
					// Return the full culture name.
					return Task.FromResult(new ProviderCultureResult(fullCulture, fullCulture));
				}
			}

			return Task.FromResult<ProviderCultureResult>(null);
		}
	}
