using Microsoft.Extensions.Caching.Memory;
using SmartWinners.Configuration;
using SmartWinners.Helpers;
using SmartWinners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class GameCategoriesService(IMemoryCache cache)
{
	private static readonly CasinoGamesApiConfiguration _config = EnvironmentHelper.CasinoGamesApiConfiguration;

	private const string CacheGroupedKey = "GameCategoriesGrouped";
	private const string CacheKeySEO = "GameCategoriesSEO";


	public async Task<List<IGrouping<int, GameCategory>>> GetGroupedCategoriesAsync(string? lang = null)
	{
		var cacheKey = $"{CacheGroupedKey}_{lang ?? "all"}";

		if (cache.TryGetValue(cacheKey, out List<IGrouping<int, GameCategory>> cachedCategories))
		{
			return cachedCategories;
		}

		var client = _config.InitClient();

		var filter = new Dictionary<string, string>();

		var response = await client.Game_Categories_GetAsync(new()
		{
			Ol_EntityId = _config.ol_EntityId,
			Ol_Username = _config.ol_UserName,
			Ol_Password = _config.ol_Password,
			Lang_Code = lang,
			Fields =
				[
						"gc.games_count",
						"gc.is_dynamic",
						"gc.enabled",
						"category_name"
				],
			FilterFields = [.. filter.Keys],
			FilterValues = [.. filter.Values]
    });

		var json = response.@return;
		var categories = JsonSerializer.Deserialize<List<GameCategory>>(json) ?? [];

		var groupedCategories = categories
				.Where(c => c.Enabled == 1)
				.OrderBy(c => c.IsDynamic)
				.GroupBy(c => c.IsDynamic)
				.ToList();

		cache.Set(cacheKey, groupedCategories, TimeSpan.FromMinutes(10));

		return groupedCategories;
	}


	public async Task<List<GameCategoryWithSEO>> GetCategoriesWithSEOAsync(string? lang, int? categoryId = null)
	{
		var cacheKey = $"{CacheKeySEO}_{lang ?? "all"}_{categoryId?.ToString() ?? "all"}";

		if (cache.TryGetValue(cacheKey, out List<GameCategoryWithSEO> cachedCategories))
		{
			return cachedCategories;
		}

		var client = _config.InitClient();

		var filter = new Dictionary<string, string>();

		if (categoryId.HasValue)
		{
			filter["categoryId"] = categoryId.Value.ToString();
		}

		var response = await client.Game_Categories_GetAsync(new()
		{
			Ol_EntityId = _config.ol_EntityId,
			Ol_Username = _config.ol_UserName,
			Ol_Password = _config.ol_Password,
			Lang_Code = lang,
			Fields =
				[
						"gc.categoryId",
						"category_name",
						"gc.games_count",
						"gc.is_dynamic",
						"gc.enabled",
						"gcs.category_id",
						"gcs.lang_code",
						"gcs.h1_title",
						"gcs.seo_title",
						"gcs.seo_description",
						"gcs.seo_keywords",
						"gcs.seo_slug",
						"gcs.og_title",
						"gcs.og_description",
						"gcs.og_image",
						"gcs.twitter_title",
						"gcs.twitter_description",
						"gcs.twitter_image",
						"gcs.meta_robots"
				],
			FilterFields = [.. filter.Keys],
			FilterValues = [.. filter.Values]
    });

		var json = response.@return;
		var categories = JsonSerializer.Deserialize<List<GameCategoryWithSEO>>(json) ?? [];

		cache.Set(cacheKey, categories, TimeSpan.FromMinutes(10));

		return categories;
	}
}