using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PalaceCasino.Agent.Client;
using System;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class GoldslotService(IMemoryCache cache, IGoldSlotApiClient api, ILogger<GoldslotService> logger) : IGoldslotService
{
	private readonly IMemoryCache _cache = cache;
	private readonly IGoldSlotApiClient _api = api;
	private readonly ILogger<GoldslotService> _logger = logger;

	public Task<long> GetUserCodeAsync(string accountId)
	{
		var cacheKey = $"usercode:{accountId}";
		return _cache.GetOrCreateAsync(cacheKey, async entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

			var resp = await _api.CreateAsync(new()
			{
				Name = accountId
			});

			if (resp.Code != 0)
			{
				_logger.LogError(
								"CreateUser failed for {AccountId}: code={Code}, msg={Msg}",
								accountId, resp.Code, resp.Message);
				throw new InvalidOperationException(
								$"Goldslot CreateUser error: {resp.Code}");
			}

			return resp.Data.User_code;
		});
	}

	public async Task<long> RefreshUserCodeAsync(string accountId)
	{
		var cacheKey = $"usercode:{accountId}";
		_cache.Remove(cacheKey);
		return await GetUserCodeAsync(accountId);
	}
	
	public async Task<_GameUrlResultData> StartGameAsync(string accountId, string gameSymbol)
	{
		var userCode = await GetUserCodeAsync(accountId);

		var resp = await _api.GameUrlAsync(new()
		{
			User_code = userCode,
			Game_symbol = gameSymbol
		});

		if (resp.Code != 0)
		{
			_logger.LogError(
					"GameUrl failed for {AccountId}, game={Game}: code={Code}, msg={Msg}",
					accountId, gameSymbol, resp.Code, resp.Message);
			throw new InvalidOperationException(
					$"Goldslot StartGame error: {resp.Code}");
		}

		return resp;
	}
}