using GoldCasino.ApiModule.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using SmartWinners.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartWinners.Controllers;

[ApiController]
[Route("")]
public class SitemapController(
    GamesService gamesService,
    GameCategoriesService categoriesService,
    StudiosService studiosService,
    ISupportedLanguagesService supportedLanguagesService,
    ILanguageSyncService languageSyncService,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    private static readonly string[] Sections = ["games", "categories", "studios"];
    private static readonly string[] DefaultLanguageFallback = ["en"];
    private const string IndexFileName = "sitemap.xml";
  
    private readonly ISupportedLanguagesService _supportedLanguagesService = supportedLanguagesService;
    private readonly string _sitemapsDirectory = Path.Combine(
        webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"),
        "sitemaps");

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> SitemapIndex()
    {
        var preGeneratedPath = TryGetPreGeneratedFile(IndexFileName);
        if (preGeneratedPath is not null)
            return PhysicalFile(preGeneratedPath, "application/xml");

        var baseUrl = BuildBaseUrl();
        var now = DateTime.UtcNow;

        var supportedLanguages = await GetNormalizedLanguagesAsync();

        var sitemapEntries = supportedLanguages
            .SelectMany(lang => Sections.Select(section => new SitemapEntry(BuildAbsoluteUrl(baseUrl, $"/sitemaps/{section}-{lang}.xml"), now)))
            .ToArray();

        var document = BuildSitemapIndex(sitemapEntries);
        return Content(document, "application/xml");
    }

    [HttpGet("sitemaps/{section}-{lang}.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> SitemapSection([FromRoute] string section, [FromRoute] string lang)
    {
        if (string.IsNullOrWhiteSpace(section))
            return NotFound();

        var normalizedSection = section.Trim().ToLowerInvariant();
        if (!Sections.Contains(normalizedSection))
            return NotFound();

        var normalizedLang = NormalizeLanguage(lang);
        var supportedLanguages = await GetNormalizedLanguagesAsync();
        var lookup = new HashSet<string>(supportedLanguages, StringComparer.OrdinalIgnoreCase);
        if (!lookup.Contains(normalizedLang))
            return NotFound();

        var staticFileName = $"{normalizedSection}-{normalizedLang}.xml";
        var preGeneratedPath = TryGetPreGeneratedFile(staticFileName);
        if (preGeneratedPath is not null)
            return PhysicalFile(preGeneratedPath, "application/xml");

        var baseUrl = BuildBaseUrl();
        IReadOnlyList<SitemapEntry> urls = normalizedSection switch
        {
            "games" => await BuildGameUrlsAsync(normalizedLang, baseUrl),
            "categories" => await BuildCategoryUrlsAsync(normalizedLang, baseUrl),
            "studios" => await BuildStudioUrlsAsync(normalizedLang, baseUrl),
            _ => Array.Empty<SitemapEntry>()
        };

        var document = BuildUrlSet(urls);
        return Content(document, "application/xml");
    }

    [HttpPost("sitemap/generate")]
    public async Task<IActionResult> GenerateSitemaps()
    {
        await languageSyncService.SyncLanguagesAsync(HttpContext?.RequestAborted ?? CancellationToken.None);

        EnsureSitemapsDirectory();

        var baseUrl = BuildBaseUrl();
        var generatedFiles = new List<string>();
        var normalizedLanguages = await GetNormalizedLanguagesAsync();

        foreach (var lang in normalizedLanguages)
        {
            foreach (var section in Sections)
            {
                IReadOnlyList<SitemapEntry> urls = section switch
                {
                    "games" => await BuildGameUrlsAsync(lang, baseUrl),
                    "categories" => await BuildCategoryUrlsAsync(lang, baseUrl),
                    "studios" => await BuildStudioUrlsAsync(lang, baseUrl),
                    _ => Array.Empty<SitemapEntry>()
                };

                var xml = BuildUrlSet(urls);
                var fileName = $"{section}-{lang}.xml";
                var filePath = GetSitemapFilePath(fileName);
                await System.IO.File.WriteAllTextAsync(filePath, xml, Encoding.UTF8);
                generatedFiles.Add(fileName);
            }
        }

        var now = DateTime.UtcNow;
        var indexEntries = normalizedLanguages
            .SelectMany(language => Sections.Select(section => new SitemapEntry(BuildAbsoluteUrl(baseUrl, $"/sitemaps/{section}-{language}.xml"), now)))
            .ToArray();

        var indexXml = BuildSitemapIndex(indexEntries);
        var indexPath = GetSitemapFilePath(IndexFileName);
        await System.IO.File.WriteAllTextAsync(indexPath, indexXml, Encoding.UTF8);

        return Ok(new
        {
            directory = "/sitemaps",
            files = generatedFiles,
            index = IndexFileName
        });
    }

    private async Task<IReadOnlyList<SitemapEntry>> BuildGameUrlsAsync(string lang, string baseUrl)
    {
        var games = await gamesService.GetAllGamesForSitemapAsync(lang);
        var langPrefix = BuildLanguagePrefix(lang);
        var urls = new List<SitemapEntry>(games.Count);

        foreach (var game in games)
        {
            if (game.Id <= 0)
                continue;

            var slugSource = string.IsNullOrWhiteSpace(game.Slug) ? game.Name : game.Slug;
            var slug = SlugHelper.ToSlug(slugSource);
            var basePath = $"{langPrefix}game/{game.Id}";
            var relative = string.IsNullOrEmpty(slug)
                ? basePath
                : $"{basePath}/{slug}";

            urls.Add(new SitemapEntry(BuildAbsoluteUrl(baseUrl, relative), null));
        }

        return urls;
    }

    private async Task<IReadOnlyList<SitemapEntry>> BuildCategoryUrlsAsync(string lang, string baseUrl)
    {
        var categories = await categoriesService.GetCategoriesWithSEOAsync(lang);
        var langPrefix = BuildLanguagePrefix(lang);
        var urls = new List<SitemapEntry>(categories.Count);
        var seen = new HashSet<int>();

        foreach (var category in categories)
        {
            if (category.CategoryID <= 0 || !category.IsActive)
                continue;

            if (!seen.Add(category.CategoryID))
                continue;

            var slugSource = string.IsNullOrWhiteSpace(category.Slug) ? category.CategoryName : category.Slug;
            var slug = SlugHelper.ToSlug(slugSource);
            var basePath = $"{langPrefix}games/{category.CategoryID}";
            var relative = string.IsNullOrEmpty(slug)
                ? basePath
                : $"{basePath}/{slug}";

            urls.Add(new SitemapEntry(BuildAbsoluteUrl(baseUrl, relative), null));
        }

        return urls;
    }

    private async Task<IReadOnlyList<SitemapEntry>> BuildStudioUrlsAsync(string lang, string baseUrl)
    {
        var studios = await studiosService.Get();
        var langPrefix = BuildLanguagePrefix(lang);
        var urls = new List<SitemapEntry>(studios.Count);

        foreach (var studio in studios)
        {
            if (studio.Id <= 0)
                continue;

            var slug = SlugHelper.ToSlug(studio.Name);
            var basePath = $"{langPrefix}games/s/{studio.Id}";
            var relative = string.IsNullOrEmpty(slug)
                ? basePath
                : $"{basePath}/{slug}";

            urls.Add(new SitemapEntry(BuildAbsoluteUrl(baseUrl, relative), null));
        }

        return urls;
    }

    private static string BuildUrlSet(IEnumerable<SitemapEntry> urls)
    {
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var urlset = new XElement(ns + "urlset",
            urls.Select(url => new XElement(ns + "url",
                new XElement(ns + "loc", url.Location),
                url.LastModified.HasValue ? new XElement(ns + "lastmod", url.LastModified.Value.ToString("yyyy-MM-dd")) : null)));

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset)
            .ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildSitemapIndex(IEnumerable<SitemapEntry> sitemaps)
    {
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var index = new XElement(ns + "sitemapindex",
            sitemaps.Select(entry => new XElement(ns + "sitemap",
                new XElement(ns + "loc", entry.Location),
                entry.LastModified.HasValue ? new XElement(ns + "lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd")) : null)));

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), index)
            .ToString(SaveOptions.DisableFormatting);
    }

    private static string BuildLanguagePrefix(string lang)
    {
        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var trimmed = lang.Trim('/');
        return $"/{trimmed}/";
    }

    private string BuildBaseUrl()
    {
        var request = HttpContext?.Request;
        if (request is null)
            throw new InvalidOperationException("HTTP context is not available.");

        var scheme = string.IsNullOrWhiteSpace(request.Scheme) ? "https" : request.Scheme;
        var host = request.Host.HasValue ? request.Host.Value : "localhost";
        return $"{scheme}://{host}".TrimEnd('/');
    }

    private static string BuildAbsoluteUrl(string baseUrl, string relative)
    {
        var sanitized = relative.StartsWith('/') ? relative : $"/{relative}";
        return $"{baseUrl}{sanitized}";
    }

    private static string NormalizeLanguage(string? lang)
    {
        return string.IsNullOrWhiteSpace(lang) ? "en" : lang.Trim().ToLowerInvariant();
    }

    private async Task<string[]> GetNormalizedLanguagesAsync()
    {
        var token = HttpContext?.RequestAborted ?? CancellationToken.None;
        var languages = await _supportedLanguagesService.GetAsync(token);

        var normalized = languages
            .Select(l => l.LanguageIso)
            .Select(NormalizeLanguage)
            .Where(code => !string.IsNullOrEmpty(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return normalized.Length > 0 ? normalized : DefaultLanguageFallback;
    }

    private void EnsureSitemapsDirectory()
    {
        Directory.CreateDirectory(_sitemapsDirectory);
    }

    private string? TryGetPreGeneratedFile(string fileName)
    {
        var path = GetSitemapFilePath(fileName);
        return System.IO.File.Exists(path) ? path : null;
    }

    private string GetSitemapFilePath(string fileName)
    {
        return Path.Combine(_sitemapsDirectory, fileName);
    }

    private sealed record SitemapEntry(string Location, DateTime? LastModified);
}
