using GoldCasino.ApiModule.Dtos.Bonuses;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWinners.Controllers;

[Route("api/ladder-bonus")]
[ApiController]
[Authorize]
public class LadderBonusController : ControllerBase
{
	private readonly IBusinessApiService _businessApiService;
	private readonly IPlayerClub365ApiService _playerClub365ApiService;
	private readonly ILogger<LadderBonusController> _logger;

	public LadderBonusController(
		IBusinessApiService businessApiService,
		IPlayerClub365ApiService playerClub365ApiService,
		ILogger<LadderBonusController> logger)
	{
		_businessApiService = businessApiService;
		_playerClub365ApiService = playerClub365ApiService;
		_logger = logger;
	}

	[HttpPost("collect")]
	public async Task<IActionResult> Collect([FromBody] LadderBonusCollectRequest request)
	{
		if (request.RecordId <= 0)
			return BadRequest(new { message = "Invalid bonus id." });

		var access = HttpContext.User.ToUserApiAccess();
		if (access is null || access.EntityId <= 0)
			return Unauthorized();

		var bonusResult = await _businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
		{
			Fields = FieldHelper<EntityBonusDto>.Fields,
			Filter = new Dictionary<string, string>
			{
				["RecordID"] = $"={request.RecordId}",
				["isDeleted"] = "=0",
				["CustomField201"] = $"={(int)BonusType.Ladder}"
			},
			LimitFrom = 0,
			LimitCount = 1
		}, access);

		if (!bonusResult.IsSuccess)
		{
			_logger.LogWarning("Failed to load ladder bonus {RecordId} for entity {EntityId}: {Error}", request.RecordId, access.EntityId, bonusResult.Error?.Message ?? bonusResult.Error?.Code);
			return StatusCode(StatusCodes.Status502BadGateway, new { message = "Unable to load bonus details. Please try again." });
		}

		var bonus = bonusResult.Value?.Data?.FirstOrDefault();
		if (bonus is null)
			return NotFound(new { message = "Bonus not found." });

		if (bonus.ParentRecordId > 0 && bonus.ParentRecordId != access.EntityId)
			return Forbid();

		if (bonus.Used.HasValue && bonus.Used.Value > 0)
			return Conflict(new { message = "Bonus already collected." });

		if (bonus.Serial is null || bonus.ValueUsd is null)
			return BadRequest(new { message = "Bonus data is incomplete." });

		if (bonus.ExpirationDate is DateTime expiry && expiry <= DateTime.UtcNow)
			return BadRequest(new { message = "Bonus has expired." });

		var nowUtc = DateTime.UtcNow;
		var expiryUtc = nowUtc.AddHours(24);

		var updateResult = await _playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
		{
			RecordId = bonus.RecordId,
			EntityId = access.EntityId,
			BonusType = bonus.Type ?? BonusType.Ladder,
			Serial = Convert.ToInt32(Math.Round(bonus.Serial.Value)),
			Value = Convert.ToDecimal(bonus.ValueUsd.Value),
			Used = 0,
			CreatedDateCustom = nowUtc,
			ExpirationDate = expiryUtc
		});

		if (!updateResult.IsSuccess)
		{
			_logger.LogError("Failed to mark ladder bonus {RecordId} as used for entity {EntityId}: {Error}", bonus.RecordId, access.EntityId, updateResult.Error?.Message ?? updateResult.Error?.Code);
			return StatusCode(StatusCodes.Status502BadGateway, new { message = "Unable to collect bonus right now. Please try again." });
		}

		return Ok(new { success = true });
	}
}

public record LadderBonusCollectRequest(int RecordId);
