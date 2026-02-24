using GoldCasino.ApiModule.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SmartWinners.Services;

public class LanguageSyncService(
    ISupportedLanguagesService supportedLanguagesService,
    ILocalizationService localizationService,
    IDomainService domainService,
    IContentService contentService,
    ILogger<LanguageSyncService> logger) : ILanguageSyncService
{
    // If your source doesn’t provide fallback per language, keep a simple rule set like your table.
    private static readonly HashSet<string> FallbackToEnglishIso2 =
        new(StringComparer.OrdinalIgnoreCase) { "he", "pt", "ru", "es", "uk" };

    private static string ToIso2(string iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return string.Empty;

        iso = iso.Trim().Replace('_', '-');

        try
        {
            var ci = CultureInfo.GetCultureInfo(iso);
            var two = ci.TwoLetterISOLanguageName; // "he-IL" -> "he"
            if (string.Equals(two, "iv", StringComparison.OrdinalIgnoreCase))
                two = "he";
            return two.ToLowerInvariant();
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task SyncLanguagesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Language Sync...");

        var rootContent = contentService.GetRootContent().FirstOrDefault();
        if (rootContent == null)
        {
            logger.LogWarning("No root content found. Skipping domain sync.");
        }

        var supported = await supportedLanguagesService.GetAsync(cancellationToken);

        // Normalize + dedupe by ISO2
        var desired = supported
            .Select(l => new
            {
                Iso2 = ToIso2(l.LanguageIso),
                Name = l.Name?.Trim() ?? string.Empty
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Iso2))
            .GroupBy(x => x.Iso2, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        // Load existing once
        var existingAll = localizationService.GetAllLanguages().ToList();

        // Build lookup by ISO2; if duplicates exist, prefer the neutral one (IsoCode == iso2)
        var existingByIso2 = existingAll
            .GroupBy(l => ToIso2(l.IsoCode), StringComparer.OrdinalIgnoreCase)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault(l => string.Equals(l.IsoCode, g.Key, StringComparison.OrdinalIgnoreCase)) ?? g.First(),
                StringComparer.OrdinalIgnoreCase);

        // Ensure English exists first (and fetch it)
        const string defaultIso2 = "en";
        if (!existingByIso2.TryGetValue(defaultIso2, out var defaultLang))
        {
            logger.LogInformation("Default language '{Iso2}' missing. Creating.", defaultIso2);
            defaultLang = new Language(defaultIso2, "English");
            localizationService.Save(defaultLang);

            existingByIso2[defaultIso2] = defaultLang;
        }

        // Domains cache (only if root exists)
        var domainsByName = new Dictionary<string, IDomain>(StringComparer.OrdinalIgnoreCase);

        if (rootContent != null)
        {
            var domains = domainService.GetAssignedDomains(rootContent.Id, includeWildcards: true)
                        ?? Enumerable.Empty<IDomain>();

            domainsByName = domains.ToDictionary(d => d.DomainName, d => d, StringComparer.OrdinalIgnoreCase);
        }

        foreach (var d in desired)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ---- LANGUAGE UPSERT ----
            if (!existingByIso2.TryGetValue(d.Iso2, out var lang))
            {
                logger.LogInformation("Creating language: {Iso2} ({Name})", d.Iso2, d.Name);

                lang = new Language(d.Iso2, d.Name);

                // fallback rule
                lang.FallbackLanguageId = (d.Iso2 != defaultIso2 && FallbackToEnglishIso2.Contains(d.Iso2))
                    ? defaultLang.Id
                    : null;
                try
                {
                    localizationService.Save(lang);
                    existingByIso2[d.Iso2] = lang;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create language {Iso2}", d.Iso2);
                }
            }
            else
            {
                var changed = false;

                // Update display name to match your DB/source name
                if (!string.IsNullOrWhiteSpace(d.Name) &&
                    !string.Equals(lang.CultureName, d.Name, StringComparison.Ordinal))
                {
                    lang.CultureName = d.Name;
                    changed = true;
                }

                // Update fallback to match rule/table
                var desiredFallbackId = (d.Iso2 != defaultIso2 && FallbackToEnglishIso2.Contains(d.Iso2))
                    ? defaultLang.Id
                    : (int?)null;

                if (lang.FallbackLanguageId != desiredFallbackId)
                {
                    lang.FallbackLanguageId = desiredFallbackId;
                    changed = true;
                }

                if (changed)
                {
                    logger.LogInformation("Updating language: {Iso2}", d.Iso2);
                    localizationService.Save(lang);
                }
            }

            // ---- DOMAIN UPSERT ----
            if (rootContent != null)
            {
                var domainName = "/" + d.Iso2; // your pattern

                if (!domainsByName.TryGetValue(domainName, out var domain))
                {
                    logger.LogInformation("Creating domain {Domain} for {Iso2}", domainName, d.Iso2);

                    try
                    {
                        var newDomain = new UmbracoDomain(domainName)
                        {
                            RootContentId = rootContent.Id,
                            LanguageId = existingByIso2[d.Iso2].Id
                        };


                        domainService.Save(newDomain);
                        domainsByName[domainName] = newDomain;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to create domain {Domain}", domainName);
                    }
                }
                else
                {
                    // If domain exists but points to wrong language, fix it
                    var desiredLangId = existingByIso2[d.Iso2].Id;
                    if (domain.LanguageId != desiredLangId)
                    {
                        logger.LogInformation("Updating domain {Domain}: LanguageId {Old} -> {New}",
                            domainName, domain.LanguageId, desiredLangId);

                        domain.LanguageId = desiredLangId;
                        domainService.Save(domain);
                    }
                }
            }
        }

        logger.LogInformation("Language Sync finished.");
    }
}
