using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class LadderBonusService
{
	private const int WelcomeVoucherSerial = 1;
	private const decimal WelcomeVoucherValue = 30m;

	private readonly IBusinessApiService _businessApiService;
	private readonly IPlayerClub365ApiService _playerClub365ApiService;
	private readonly ILogger<LadderBonusService> _logger;

	public LadderBonusService(
		IBusinessApiService businessApiService,
		IPlayerClub365ApiService playerClub365ApiService,
		ILogger<LadderBonusService> logger)
	{
		_businessApiService = businessApiService;
		_playerClub365ApiService = playerClub365ApiService;
		_logger = logger;
	}

	public async Task EnsureWelcomeVoucherAsync(UserApiAccess? access, CancellationToken cancellationToken = default)
	{
		if (access is null || access.EntityId <= 0)
			return;

		var bonusesResult = await _businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
		{
			Filter = new Dictionary<string, string>
			{
				["isDeleted"] = "=0",
				["CustomField201"] = $"={(int)BonusType.Ladder}",
				["ORDER BY"] = "CreatedDate desc"
			},
			LimitFrom = 0,
			LimitCount = 50
		}, access);

		if (!bonusesResult.IsSuccess)
		{
			_logger.LogWarning("Unable to fetch ladder bonuses for EntityId {EntityId}: {Error}", access.EntityId, bonusesResult.Error?.Message ?? bonusesResult.Error?.Code);
			return;
		}

		var hasWelcomeVoucher = bonusesResult.Value?.Data?.Any(b =>
			b.Type == BonusType.Ladder &&
			b.Serial.HasValue &&
			Convert.ToInt32(Math.Round(b.Serial.Value)) == WelcomeVoucherSerial &&
			(b.IsDeleted ?? 0) == 0) ?? false;

		if (hasWelcomeVoucher)
			return;

		var updateResult = await _playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
		{
			RecordId = 0,
			EntityId = access.EntityId,
			BonusType = BonusType.Ladder,
			Serial = WelcomeVoucherSerial,
			Value = WelcomeVoucherValue,
			CreatedDateCustom = DateTime.UtcNow,
			Used = 0,
			IsDeleted = false
		});

		if (!updateResult.IsSuccess)
		{
			_logger.LogError("Failed to create ladder welcome voucher for EntityId {EntityId}: {Error}", access.EntityId, updateResult.Error?.Message ?? updateResult.Error?.Code);
		}
	}
}
