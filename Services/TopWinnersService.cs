using GoldCasino.ApiModule.Convertors;
using Microsoft.Extensions.Caching.Memory;
using SmartWinners.Helpers;
using SmartWinners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Services;

public class TopWinnersService(UmbracoHelper umbracoHelper, IMemoryCache cache)
{

	public async Task<TopWinnersListModel> GetTopWinners(int count = 10, string langISO = "en", TopWinnersFilterType filterType = TopWinnersFilterType.Latest)
	{
		var cacheKey = $"TopWinners_{count}_{langISO}_{filterType}";

		if (cache.TryGetValue(cacheKey, out TopWinnersListModel cached))
		{
			var hashed = cached?.GetHashCode();
			return cached;
		}

		var config = EnvironmentHelper.CasinoGamesApiConfiguration;
		var client = config.InitClient();

		var apiResp = await client.Winners_GetAsync(new()
		{
			Ol_EntityId = config.ol_EntityId,
			Ol_Username = config.ol_UserName,
			Ol_Password = config.ol_Password,
			Lang_Code = langISO,
			Fields = [
				"game_name",
				"game_image",
				"CountryISO",
				"FirstName",
				"LastName",
				"DateTime",
				"bet",
				"win",
				"CountryName",
			],
			FilterFields = [],
			FilterValues = [],
			LimitFrom = 0,
			LimitCount = count,
		});

		var winners = JsonSerializer.Deserialize<List<TopWinnersGetResponse>>(apiResp.@return) ?? [];
		var langPrefix = langISO.ToLowerInvariant() switch
		{
			"en" => "",
			_ => $"/{langISO.ToLowerInvariant()}"
		};

		var grouped = winners
		.Select(w =>
		{

			// var wonDate = DateTime.Parse(w.DateTime, CultureInfo.InvariantCulture);
			var wonTimeSpan = DateTime.UtcNow - w.DateTime;
			var cultureInfo = Thread.CurrentThread.CurrentCulture;
			var Slug = string.IsNullOrEmpty(w.GameName)
				? "" : string.Join("-",
				w.GameName
				.ToLowerInvariant()
				.Trim()
				// normalize to spaces first so we can split on them
				.Replace('+', ' ')
				.Replace("&", " and ")
				.Replace('/', ' ')
				.Replace('\\', ' ')
				.Replace('.', ' ')
				.Split(' ', StringSplitOptions.RemoveEmptyEntries))
				// strip remaining characters that commonly break routes or look bad in slugs
				.Replace("?", "")
				.Replace("%", "")
				.Replace("#", "")
				.Replace("\"", "")
				.Replace("'", "");
			return new TopWinnersModel
			{
				GameUrl = $"{langPrefix}/game/{w.gameId}/{Slug}",
				GameName = $"{w.GameName}",
				GameImage = $"{w.GameImage}",
				UserName = MaskUsername(w.FirstName, w.LastName),
				CountryISO = w.CountryISO,
				// WonDate = DateTime.Parse(w.DateTime),
				WonDate = w.DateTime,
				BetUsd = w.Bet,
				ProfitUsd = w.Win,
				Multiplayer = w.Bet > 0 ? Math.Round(w.Win / w.Bet, 2) : 0,
				TimeString = wonTimeSpan.TotalSeconds < 60
									? umbracoHelper.GetDictionaryValueOrDefault("a few seconds ago", "a few seconds ago")
									: w.DateTime.ToString("ddd dd yyyy HH:mm", cultureInfo)
				//TimeString = umbracoHelper.GetDictionaryValueOrDefault("a few seconds ago", "a few seconds ago")
			};
		})
		.ToList();

		var result = new TopWinnersListModel
		{
			List = grouped,
			Count = count,
			FilterType = filterType
		};
		cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));

		return result;
	}
	private static string MaskUsername(string firstname, string lastName)
	{
		if (!string.IsNullOrEmpty(firstname))
			firstname = $"{firstname}*******";

		if (!string.IsNullOrEmpty(lastName))
			lastName = $"{lastName}*******";

		return $"{firstname} {lastName}";
	}
}

public class TopWinnersGetResponse
{
	[JsonPropertyName("game_name")] public string GameName { get; set; } = string.Empty;
	[JsonPropertyName("game_id")] public int gameId { get; set; } = 0;
	[JsonPropertyName("game_image")] public string GameImage { get; set; } = string.Empty;
	[JsonPropertyName("CountryISO")] public string CountryISO { get; set; } = string.Empty;
	[JsonPropertyName("FirstName")] public string FirstName { get; set; } = string.Empty;
	[JsonPropertyName("LastName")] public string LastName { get; set; } = string.Empty;
	[JsonConverter(typeof(CustomDateTimeConverter))]
	[JsonPropertyName("DateTime")] public DateTime DateTime { get; set; }
	[JsonPropertyName("bet")] public decimal Bet { get; set; }
	[JsonPropertyName("win")] public decimal Win { get; set; }
	[JsonPropertyName("CountryName")] public string CountryName { get; set; } = string.Empty;
}