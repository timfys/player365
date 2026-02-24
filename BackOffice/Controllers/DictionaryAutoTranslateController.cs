using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace SmartWinners.BackOffice.Controllers;

[PluginController("AiTools")]
public class DictionaryAutoTranslateController(ILocalizationService loc, ITranslator translator) : UmbracoAuthorizedApiController
{

  // POST /umbraco/backoffice/AiTools/DictionaryAutoTranslate/Run
  [HttpPost]
  public async Task<IActionResult> Run([FromBody] RunRequest req, CancellationToken ct)
  {
    var sourceIso2 = (req.SourceIso2 ?? "en").Trim().ToLowerInvariant();

    var languages = loc.GetAllLanguages().ToList();
    var sourceLang = languages.FirstOrDefault(l => l.IsoCode.Equals(sourceIso2, StringComparison.OrdinalIgnoreCase));
    if (sourceLang is null)
      return BadRequest($"Source language '{sourceIso2}' not found.");

    var allItems = loc.GetDictionaryItemDescendants(null).ToList();

    var stats = new Stats();

    foreach (var item in allItems)
    {
      ct.ThrowIfCancellationRequested();

      var sourceText = GetValue(item, sourceIso2);
      if (string.IsNullOrWhiteSpace(sourceText))
      {
        stats.SkippedNoSource++;
        continue;
      }

      foreach (var lang in languages)
      {
        var targetIso2 = lang.IsoCode.ToLowerInvariant();
        if (targetIso2 == sourceIso2)
          continue;

        var existing = GetValue(item, targetIso2);
        if (req.OnlyFillMissing && !string.IsNullOrWhiteSpace(existing))
        {
          stats.SkippedExisting++;
          continue;
        }

        var translated = await translator.TranslateAsync(sourceText, sourceIso2, targetIso2, ct);
        translated = PlaceholderSafeOrFallback(sourceText, translated);

        loc.AddOrUpdateDictionaryValue(item, lang, translated);

        stats.Translated++;
        if (req.DelayMs > 0)
          await Task.Delay(req.DelayMs, ct);
      }

      loc.Save(item);
    }

    return Ok(stats);
  }

  private static string? GetValue(IDictionaryItem item, string iso2)
      => item.Translations
          .FirstOrDefault(t => t.LanguageIsoCode.Equals(iso2, StringComparison.OrdinalIgnoreCase))
          ?.Value;

  private static string PlaceholderSafeOrFallback(string source, string translated)
  {
    // Keep "{...}" tokens count stable (extend if you also use "%s", "{{...}}", etc.)
    var rx = new Regex(@"\{[^}]+\}", RegexOptions.Compiled);
    var s = rx.Matches(source).Select(m => m.Value).ToList();
    var t = rx.Matches(translated).Select(m => m.Value).ToList();

    if (s.Count != t.Count)
      return source; // safest: do not break runtime formatting

    return translated;
  }

  public sealed record RunRequest(string? SourceIso2 = "en", bool OnlyFillMissing = true, int DelayMs = 0);
  
  public sealed class Stats
  {
    public int Translated { get; set; }
    public int SkippedExisting { get; set; }
    public int SkippedNoSource { get; set; }
  }
}
