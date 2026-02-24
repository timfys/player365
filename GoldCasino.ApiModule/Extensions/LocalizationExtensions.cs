//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Localization;
//using Microsoft.Extensions.DependencyInjection;
//using System.Globalization;
//using Microsoft.AspNetCore.Http;

//namespace GoldCasino.ApiModule.Extensions;

//public static class LocalizationExtensions
//{
//  private static readonly CultureInfo[] SupportedCultures =
//  [
//    new CultureInfo("en-US"),
//    new CultureInfo("fr-FR"),
//    new CultureInfo("es-ES"),
//    new CultureInfo("ru-RU"),
//    new CultureInfo("uk-UA"),
//    new CultureInfo("he-IL")
//  ];

//  public static IServiceCollection AddCustomLocalization(this IServiceCollection services)
//  {
//    services.AddLocalization();
//    return services;
//  }

//  public static IApplicationBuilder UseCustomRequestLocalization(this IApplicationBuilder app)
//  {
//    var options = new RequestLocalizationOptions
//    {
//      DefaultRequestCulture = new RequestCulture("en-US"),
//      SupportedCultures = SupportedCultures,
//      SupportedUICultures = SupportedCultures,
//      RequestCultureProviders =
//      {
//        new UrlSegmentRequestCultureProvider(),
//        new QueryStringRequestCultureProvider(),
//        new CookieRequestCultureProvider { CookieName = "Culture" },
//        new AcceptLanguageHeaderRequestCultureProvider()
//      }
//    };

//    options.AddInitialRequestCultureProvider(new UrlSegmentRequestCultureProvider());
//    return app.UseRequestLocalization(options);
//  }
//}

//public sealed class UrlSegmentRequestCultureProvider : RequestCultureProvider
//{
//  private static readonly Dictionary<string, string> CultureMap = new(StringComparer.OrdinalIgnoreCase)
//  {
//    { "en", "en-US" },
//    { "fr", "fr-FR" },
//    { "es", "es-ES" },
//    { "ru", "ru-RU" },
//    { "uk", "uk-UA" },
//    { "he", "he-IL" }
//  };

//  public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
//  {
//    ArgumentNullException.ThrowIfNull(httpContext);
//    var path = httpContext.Request.Path.Value;

//    Console.WriteLine($"UrlSegmentRequestCultureProvider: path = {path}");
//    if (string.IsNullOrEmpty(path) || path == "/")
//      return Task.FromResult<ProviderCultureResult?>(null);

//    var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
//    if (segments.Length == 0) return Task.FromResult<ProviderCultureResult?>(null);

//    var first = segments[0];
//    if (CultureMap.TryGetValue(first, out var full))
//      return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(full, full));
    
//    return Task.FromResult<ProviderCultureResult?>(null);
//  }
//}
