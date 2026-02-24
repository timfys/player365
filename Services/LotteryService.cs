using GoldCasino.ApiModule.Convertors;
using Microsoft.Extensions.Caching.Memory;
using SmartWinners.Configuration;
using SmartWinners.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class LotteryService(IMemoryCache cache)
{
	private static readonly SmartWinnersApiConfiguration _config = EnvironmentHelper.SmartWinnersApiConfiguration;

	private const string CacheKeyAll = "Lotteries_All";
	private const string CacheKeyBiggest = "Lotteries_Biggest";

	// CA1869: Cache and reuse JsonSerializerOptions instance
	private static readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

	private static JsonSerializerOptions CreateJsonOptions()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		options.Converters.Add(new CustomDateTimeConverter());
		return options;
	}

	public async Task<List<LotteryModel>> GetLotteriesAsync()
	{
		if (cache.TryGetValue(CacheKeyAll, out List<LotteryModel> cached))
		{
			return cached;
		}

		var client = _config.InitClient();
		var result = await client.Lotteries_GetAsync(new()
		{
			Password = _config.Password,
			Fields =
				[
					"l.LotteryName",
					"l.CountryName",
					"l.Next_Jackpot",
					"l.Next_Draw_Date",
					"l.IsActive",
					"c.Symbol"
				],
			FilterFields = ["IsActive"],
			FilterValues = ["1"],
		});

		var lotteries = JsonSerializer.Deserialize<List<LotteryModel>>(result.@return, _jsonOptions) ?? [];

		cache.Set(CacheKeyAll, lotteries, TimeSpan.FromMinutes(10));

		return lotteries;
	}

	public async Task<LotteryModel?> GetBiggestLotteryAsync()
	{
		if (cache.TryGetValue(CacheKeyBiggest, out LotteryModel cached))
		{
			return cached;
		}

		var lotteries = await GetLotteriesAsync();
		var biggest = lotteries
				.OrderByDescending(l => l.Next_Jackpot)
				.FirstOrDefault();

		if (biggest != null)
		{
			cache.Set(CacheKeyBiggest, biggest, TimeSpan.FromMinutes(10));
		}

		return biggest;
	}
}

public class LotteryModel
{
	[JsonPropertyName("LotteryId")]
	public int LotteryId { get; set; }

	[JsonPropertyName("LotteryName")]
	public string LotteryName { get; set; } = string.Empty;

	[JsonPropertyName("CountryName")]
	public string CountryName { get; set; } = string.Empty;

	[JsonPropertyName("Next_Jackpot")]
	public decimal Next_Jackpot { get; set; }

	[JsonPropertyName("Next_Draw_Date")]
	public DateTime Next_Draw_Date { get; set; }

	[JsonPropertyName("Symbol")]
	public string Symbol { get; set; } = string.Empty;	

	[JsonPropertyName("IsActive")]
	public int IsActive { get; set; }

	[JsonIgnore]
	public bool Active => IsActive == 1;
}