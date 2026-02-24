using GoldCasino.ApiModule.Dtos.Games;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.Extensions.Caching.Memory;
using SmartWinners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SmartWinners.Helpers;

namespace SmartWinners.Services;

public record GamesListParameters
{
	public string Lang { get; init; } = "";
	public int Start { get; init; } = 0;
	public int Max { get; init; } = 0;
	public int CategoryId { get; init; }
	public string CategoryName { get; init; } = "";
	public string Query { get; init; } = "";
	public int Page { get; init; } = 1;
	public int PageSize { get; init; } = 30;
	public int EntityId { get; init; }
	public string Password { get; init; } = "";
	public string? Username { get; init; }
	public string? FilterVal { get; init; }

	public string? SortBy { get; init; }  // id | name | provider
	public string? SortDir { get; init; }

	public IReadOnlyList<KeyValuePair<string, string>>? Filters { get; init; }
}

public class GamesService(IMemoryCache cache, IPlayerClub365ApiService client, IIpInfoCountryResolver ipCountryResolver, IHttpContextAccessor httpContextAccessor)
{

	public async Task<GamesListModel> GetList<T>(GamesListParameters parameters) where T : class
	{
		parameters = Normalize(parameters);

		if (parameters.CategoryId is 1 or 2)
			return await FetchAndShape<T>(parameters);

		var cacheKey = BuildCacheKey(parameters);
		if (cache.TryGetValue(cacheKey, out GamesListModel cached))
			return cached;

		var result = await FetchAndShape<T>(parameters);
		cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
		return result;
	}

	private static GamesListParameters Normalize(GamesListParameters p)
	{
		var page = p.Page <= 0 ? 1 : p.Page;
		var pageSize = p.PageSize <= 0 ? 30 : p.PageSize;
		if (pageSize > 200) pageSize = 200;

		return p with
		{
			Page = page,
			PageSize = pageSize,
			Start = (page - 1) * pageSize + 1,
			Max = pageSize,
			SortBy = p.SortBy?.ToLowerInvariant(),
			SortDir = p.SortDir?.ToLowerInvariant()
		};
	}

	private static string BuildOrderClause(GamesListParameters p)
	{
		var col = p.SortBy switch
		{
			"name" => "pg.game_name",
			"provider" => "pg.provider_id",
			_ => "pg.gameID"
		};
		var dir = p.SortDir == "desc" ? "DESC" : "ASC";
		var secondary = col == "pg.gameID" ? "" : ", pg.gameID ASC";
		return $"{col} {dir}{secondary}";
	}

	private static string BuildCacheKey(GamesListParameters p)
	{
		var cat = (p.CategoryName ?? "").Replace(' ', '-');

		// include filters & order for correctness
		var pairs = new List<KeyValuePair<string, string>>();
		if (p.Filters is not null) pairs.AddRange(p.Filters);

		if (p.SortDir is not null && p.SortBy is not null)
		{
			var orderBy = BuildOrderClause(p);
			pairs.Add(new("ORDER BY", orderBy));
		}
		// build stable string
		var filterKey = string.Join("|", pairs.Select(kv => $"{kv.Key}={kv.Value}"));

		return $"games:list:{p.Lang}:{p.CategoryId}:{cat}:{p.Start}:{p.Max}:{filterKey}";
	}

	private async Task<GamesListModel> FetchAndShape<T>(GamesListParameters p) where T : class
	{
		var raw = await FetchGames<T>(p);
		raw.Query = p.Query;
		raw.Page = p.Page;
		raw.PageSize = p.PageSize;
		raw.SortBy = p.SortBy;
		raw.SortDir = p.SortDir;
		raw.HasMore = raw.List.Count == p.PageSize;
		return raw;
	}

	private async Task<GamesListModel> FetchGames<T>(GamesListParameters p) where T : class
	{
		// preserve insertion order; don’t rely on Dictionary ordering
		var pairs = new Dictionary<string, string>
		{
			{ "pg.launch_enable", "1" },
			{"countryISO", IdentityHelper.GetUserIsoFromCloudFlare(httpContextAccessor.HttpContext)?.ToLowerInvariant() ?? 
			               await ipCountryResolver.GetCountryIsoAsync(IdentityHelper.GetUserIp(httpContextAccessor.HttpContext)) ?? 
			               "us"},
		};

		if (!string.IsNullOrEmpty(p.FilterVal))
			pairs.Add("pg.game_name", p.FilterVal);

		if (p.Filters is not null && p.Filters.Count > 0)
			foreach (var filter in p.Filters)
				pairs.Add(filter.Key, filter.Value);

		if (p.CategoryId != 0)
			pairs.Add("categoryID", $"{p.CategoryId}");

		// if (p.CategoryId is 1 or 2)
		// {
		// 	pairs.Add("entityId", $"{p.EntityId}");
		// 	pairs.Add("ol_password", $"{p.Password}");
		// }

		if (p.SortDir is not null && p.SortBy is not null)
		{
			var orderBy = BuildOrderClause(p);
			pairs.Add("ORDER BY", orderBy);
		}

		var apiResp = await client.GamesGetAsync(new()
		{
			EntityId = p.EntityId,
			Username = p.Username,
			Password = p.Password,
			LangCode = string.IsNullOrEmpty(p.Lang) ? "en" : p.Lang,
			Fields = FieldHelper<T>.Fields,
			Filter = pairs,
			LimitFrom = p.Start,
			LimitCount = p.Max
		});

		var games = apiResp.Value?.Games ?? [];

		return new GamesListModel
		{
			List = games,
			CategoryID = p.CategoryId,
			ViewLink = $"/games/{p.CategoryId}/{p.CategoryName}"
		};
	}

	public async Task<GamesListModel> GetByStudio(string lang, int studioId, int start = 0, int max = 0)
	{
		string cacheKey = $"games:studio:{lang}:{studioId}:{start}:{max}";

		if (cache.TryGetValue(cacheKey, out GamesListModel cached))
		{
			return cached;
		}

		var filter = new Dictionary<string, string>
		{
			{ "pg.launch_enable", "1" },
			{ "pg.provider_id", $"{studioId}" },
			{ "Hall_Balance", ">0" },
			{"countryISO", IdentityHelper.GetUserIsoFromCloudFlare(httpContextAccessor.HttpContext)?.ToLowerInvariant() ?? 
			               await ipCountryResolver.GetCountryIsoAsync(IdentityHelper.GetUserIp(httpContextAccessor.HttpContext)) ?? 
			               "us"},
		};

		var apiResp = await client.GamesGetAsync(new()
		{
			LangCode = string.IsNullOrEmpty(lang) ? "en" : lang,
			Fields = FieldHelper<GameSimpleDto>.Fields,
			Filter = filter,
			LimitFrom = start,
			LimitCount = max
		});

		var games = apiResp.Value?.Games ?? [];

		var result = new GamesListModel
		{
			List = games
		};

		cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

		return result;
	}

	public async Task<Game?> GetByIdCode(string lang, int id, Microsoft.AspNetCore.Http.HttpContext? httpContext = null)
	{
		var filter = new Dictionary<string, string>(){
						{"pg.launch_enable", "1"},
						{"pg.GameID", id.ToString()},
						{"countryISO", IdentityHelper.GetUserIsoFromCloudFlare(httpContext)?.ToLowerInvariant() ?? 
						                await ipCountryResolver.GetCountryIsoAsync(IdentityHelper.GetUserIp(httpContext)) ?? 
						                "us"},
				};

		var req = new GamesGetRequest()
		{
			LangCode = string.IsNullOrEmpty(lang) ? "en" : lang,
			Fields = FieldHelper<GameDetailedDto>.Fields,
			Filter = filter,
			LimitFrom = 0,
			LimitCount = 1
		};

		if (httpContext is not null && httpContext.User.ToUserApiAccess() is UserApiAccess user)
		{
			req.EntityId = user.EntityId;
			req.Username = user.Username;
			req.Password = user.Password;
		}

		var games = await client.GamesGetAsync(req);

		var result = games.Value?.Games?.FirstOrDefault();
		return result;
	}

	public async Task<Game?> GetByIdCode(string lang, int id, string ipAddress, Microsoft.AspNetCore.Http.HttpContext? httpContext = null)
	{

		var filter = new Dictionary<string, string>(){
						{"pg.launch_enable", "1"},
						{"pg.GameID", id.ToString()},
						{"VisitorIP", ipAddress},
						{"countryISO", IdentityHelper.GetUserIsoFromCloudFlare(httpContext)?.ToLowerInvariant() ?? 
						               await ipCountryResolver.GetCountryIsoAsync(IdentityHelper.GetUserIp(httpContext)) ?? 
						               "us"},
				};

		var req = new GamesGetRequest()
		{
			LangCode = string.IsNullOrEmpty(lang) ? "en" : lang,
			Fields = FieldHelper<GameDetailedDto>.Fields,
			Filter = filter,
			LimitFrom = 0,
			LimitCount = 1
		};

		if (httpContext is not null && httpContext.User.ToUserApiAccess() is UserApiAccess user)
		{
			req.EntityId = user.EntityId;
			req.Username = user.Username;
			req.Password = user.Password;
		}

		var games = await client.GamesGetAsync(req);

		var result = games.Value?.Games?.FirstOrDefault();
		return result;
	}

	public async Task<IReadOnlyList<Game>> GetAllGamesForSitemapAsync(string? langIso = null, int batchSize = 500)
	{
		var lang = string.IsNullOrWhiteSpace(langIso) ? "en" : langIso.ToLowerInvariant();
		var chunkSize = batchSize <= 0 ? 500 : batchSize;
		var cacheKey = $"games:sitemap:{lang}:{chunkSize}";

		if (cache.TryGetValue(cacheKey, out IReadOnlyList<Game> cached))
			return cached;

		var gamesForLang = new List<Game>();
		var seenIds = new HashSet<int>();
		var offset = 0;

		while (true)
		{
			var response = await client.GamesGetAsync(new()
			{
				LangCode = lang,
				Fields = FieldHelper<GameDetailedDto>.Fields,
				Filter = new Dictionary<string, string> {
					{"pg.launch_enable", "1"},
					{"countryISO", IdentityHelper.GetUserIsoFromCloudFlare(httpContextAccessor.HttpContext)?.ToLowerInvariant() ?? 
					                await ipCountryResolver.GetCountryIsoAsync(IdentityHelper.GetUserIp(httpContextAccessor.HttpContext)) ?? 
					                "us"},
				 },
				LimitFrom = offset,
				LimitCount = chunkSize
			});

			var batch = response.Value?.Games ?? [];
			if (batch.Count == 0)
				break;

			foreach (var game in batch)
			{
				if (game.Enabled != 1)
					continue;
				if (seenIds.Add(game.Id))
					gamesForLang.Add(game);
			}

			if (batch.Count < chunkSize)
				break;

			offset += chunkSize;
		}

		cache.Set(cacheKey, gamesForLang, TimeSpan.FromMinutes(10));
		return gamesForLang;
	}
}
