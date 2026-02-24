using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace SmartWinners.BackOffice.Controllers;

[PluginController("AiTools")]
public sealed class DictionaryAuditController(ILocalizationService loc, IHostEnvironment env)
	: UmbracoAuthorizedApiController
{
	public sealed record CreateMissingRequest(
		string? Root = null,
		bool DefaultValueFromKey = false,
		bool SetEnglishValueFromKey = true,
		string EnglishIso2 = "en",
		Guid? ParentId = null);

	public sealed record DeleteUnusedRequest(
		string? Root = null,
		Guid? UnderParentId = null,
		bool DryRun = true,
		bool OnlyLeaf = true);

	// GET /umbraco/backoffice/AiTools/DictionaryAudit/Scan
	// Scans source files for GetDictionaryValue("...") / GetDictionaryOrDefault("...") string-literal usages.
	[HttpGet]
	public IActionResult Scan([FromQuery] string? root = null)
	{
		var result = BuildAuditResult(root);
		return result.BadRequest is not null
			? BadRequest(result.BadRequest)
			: Ok(result.Payload);
	}

	// POST /umbraco/backoffice/AiTools/DictionaryAudit/CreateMissing
	// Creates missing dictionary items under the requested parent (or root when ParentId is null).
	[HttpPost]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	public IActionResult CreateMissing([FromBody] CreateMissingRequest req)
	{
		var audit = BuildAuditResult(req.Root);
		if (audit.BadRequest is not null)
			return BadRequest(audit.BadRequest);

		var missingKeys = ((IEnumerable<string>)audit.Payload.MissingKeysList).ToList();
		var created = new List<string>(missingKeys.Count);
		var failed = new List<object>();

		Umbraco.Cms.Core.Models.ILanguage? enLang = null;
		if (req.SetEnglishValueFromKey)
		{
			try
			{
				var iso = (req.EnglishIso2 ?? "en").Trim();
				enLang = loc.GetLanguageByIsoCode(iso);
			}
			catch
			{
				enLang = null;
			}
		}

		foreach (var key in missingKeys)
		{
			try
			{
				var defaultValue = req.DefaultValueFromKey ? key : null;
				var item = loc.CreateDictionaryItemWithIdentity(key, req.ParentId, defaultValue);

				// If defaultValue is used, Umbraco assigns it to all languages.
				// If you only want English to get the key text, keep defaultValue null and set EN explicitly.
				if (enLang is not null)
				{
					loc.AddOrUpdateDictionaryValue(item, enLang, key);
					loc.Save(item);
				}

				created.Add(key);
			}
			catch (Exception ex)
			{
				failed.Add(new { Key = key, Error = ex.Message });
			}
		}

		// Return refreshed counts after creation (best-effort; failures may remain missing).
		var after = BuildAuditResult(req.Root);

		return Ok(new
		{
			CreatedCount = created.Count,
			CreatedKeys = created,
			FailedCount = failed.Count,
			Failed = failed,
			Before = audit.Payload,
			After = after.Payload
		});
	}

	// POST /umbraco/backoffice/AiTools/DictionaryAudit/DeleteUnused
	// Deletes dictionary items not referenced in scanned code.
	// Safety defaults: DryRun=true and OnlyLeaf=true.
	[HttpPost]
	[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
	public IActionResult DeleteUnused([FromBody] DeleteUnusedRequest req)
	{
		var audit = BuildAuditResult(req.Root);
		if (audit.BadRequest is not null)
			return BadRequest(audit.BadRequest);

		var usedKeys = new HashSet<string>(
			((IEnumerable<string>)audit.Payload.DistinctKeysList),
			StringComparer.OrdinalIgnoreCase);

		var candidates = loc
			.GetDictionaryItemDescendants(req.UnderParentId)
			.Where(i => !string.IsNullOrWhiteSpace(i.ItemKey))
			.Where(i => !usedKeys.Contains(i.ItemKey))
			.ToList();

		var leafCandidates = new List<object>();
		var skippedParents = new List<object>();
		var deleted = new List<object>();
		var failed = new List<object>();

		foreach (var item in candidates)
		{
			bool hasChildren;
			try
			{
				hasChildren = loc.GetDictionaryItemChildren(item.Key).Any();
			}
			catch
			{
				hasChildren = true; // be safe: don't delete if unsure
			}

			if (req.OnlyLeaf && hasChildren)
			{
				skippedParents.Add(new { Key = item.ItemKey });
				continue;
			}

			leafCandidates.Add(new { Key = item.ItemKey });
			if (req.DryRun)
				continue;

			try
			{
				loc.Delete(item);
				deleted.Add(new { Key = item.ItemKey });
			}
			catch (Exception ex)
			{
				failed.Add(new { Key = item.ItemKey, Error = ex.Message });
			}
		}

		return Ok(new
		{
			DryRun = req.DryRun,
			OnlyLeaf = req.OnlyLeaf,
			ScanRoot = audit.Payload.ScanRoot,
			UsedKeysCount = usedKeys.Count,
			CandidateUnusedCount = candidates.Count,
			UnusedLeafCount = leafCandidates.Count,
			SkippedParentsCount = skippedParents.Count,
			DeletedCount = deleted.Count,
			FailedCount = failed.Count,
			UnusedLeaf = leafCandidates,
			SkippedParents = skippedParents,
			Deleted = deleted,
			Failed = failed
		});
	}

	private (string? BadRequest, dynamic Payload) BuildAuditResult(string? root)
	{
		var contentRoot = env.ContentRootPath;
		var scanRoot = string.IsNullOrWhiteSpace(root)
			? contentRoot
			: Path.GetFullPath(Path.Combine(contentRoot, root));

		// Safety: do not allow scanning outside the app.
		if (!scanRoot.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
			return ("Invalid root path.", new { });

		var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var item in loc.GetDictionaryItemDescendants(null))
		{
			if (!string.IsNullOrWhiteSpace(item.ItemKey))
				existingKeys.Add(item.ItemKey);
		}

		var occurrences = new List<Occurrence>(capacity: 1024);
		foreach (var file in EnumerateFiles(scanRoot))
		{
			ScanFile(file, contentRoot, occurrences);
		}

		var distinctKeys = occurrences
			.Select(o => o.Key)
			.Where(k => !string.IsNullOrWhiteSpace(k))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var missingKeys = distinctKeys
			.Where(k => !existingKeys.Contains(k))
			.ToList();

		var missingOccurrences = occurrences
			.Where(o => missingKeys.Contains(o.Key, StringComparer.OrdinalIgnoreCase))
			.OrderBy(o => o.File, StringComparer.OrdinalIgnoreCase)
			.ThenBy(o => o.Line)
			.ToList();

		return (null, new
		{
			ScanRoot = scanRoot,
			TotalOccurrences = occurrences.Count,
			DistinctKeys = distinctKeys.Count,
			DistinctKeysList = distinctKeys,
			MissingKeys = missingKeys.Count,
			MissingKeysList = missingKeys,
			MissingOccurrences = missingOccurrences
		});
	}

	private static IEnumerable<string> EnumerateFiles(string scanRoot)
	{
		var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cshtml", ".cs" };
		var excludedDirNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"bin", "obj", "node_modules", ".git", ".vs", "wwwroot"
		};

		foreach (var file in Directory.EnumerateFiles(scanRoot, "*.*", SearchOption.AllDirectories))
		{
			var ext = Path.GetExtension(file);
			if (!allowedExtensions.Contains(ext))
				continue;

			var dir = Path.GetDirectoryName(file);
			if (dir is not null)
			{
				var parts = dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				if (parts.Any(p => excludedDirNames.Contains(p)))
					continue;
			}

			yield return file;
		}
	}

	private static void ScanFile(string filePath, string contentRoot, List<Occurrence> occurrences)
	{
		var patterns = new[]
		{
			new CallPattern("GetDictionaryValue", BuildCallRegex("GetDictionaryValue")),
			new CallPattern("GetDictionaryValueOrDefault", BuildCallRegex("GetDictionaryValueOrDefault")),
			new CallPattern("GetDictionaryOrDefault", BuildCallRegex("GetDictionaryOrDefault")),
		};

		var relPath = Path.GetRelativePath(contentRoot, filePath);
		string[] lines;
		try
		{
			lines = System.IO.File.ReadAllLines(filePath);
		}
		catch
		{
			return;
		}

		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			foreach (var p in patterns)
			{
				foreach (Match m in p.Regex.Matches(line))
				{
					var key = ExtractStringLiteral(m.Groups["str"].Value, m.Groups["verbatim"].Success);
					if (string.IsNullOrWhiteSpace(key))
						continue;

					occurrences.Add(new Occurrence(
						Key: key,
						File: relPath.Replace('\\', '/'),
						Line: i + 1,
						Call: p.Name));
				}
			}
		}
	}

	private static Regex BuildCallRegex(string methodName)
	{
		// Matches the first argument when it is a string literal:
		// - MethodName("...")
		// - MethodName(@"...")
		// Group 'str' contains the *inner* string contents (no surrounding quotes).
		var pattern =
			$@"\b{Regex.Escape(methodName)}(?!\w)\s*\(\s*(?:(?<verbatim>@)""(?<str>(?:[^""]|"""")*)""|""(?<str>(?:\\.|[^""\\])*)"")\s*(?=,|\))";
		return new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
	}

	private static string ExtractStringLiteral(string raw, bool isVerbatim)
	{
		if (isVerbatim)
		{
			// Verbatim string: "" escapes a quote.
			return raw.Replace("\"\"", "\"");
		}

		// Regular C# string: conservative unescape.
		return raw
			.Replace("\\\"", "\"")
			.Replace("\\\\", "\\");
	}

	private sealed record CallPattern(string Name, Regex Regex);
	public sealed record Occurrence(string Key, string File, int Line, string Call);
}
