
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Services;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SmartWinners.Controllers;

public class EmailVerificationController(
	UserService userService,
	IBusinessApiService businessApiService,
	IPlayerClub365ApiService playerClub365ApiService,
	ILogger<EmailVerificationController> logger) : Controller
{
	private readonly UserService _userService = userService;
	private readonly IBusinessApiService _businessApiService = businessApiService;
	private readonly IPlayerClub365ApiService _playerClub365ApiService = playerClub365ApiService;
	private readonly ILogger<EmailVerificationController> _logger = logger;

	private const decimal EmailVerificationBonusValue = 5m;

	// GET /verify-email?vc=...&eID=...
	[HttpGet("verify-email")]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public async Task<IActionResult> VerifyEmail([FromQuery] string? vc, [FromQuery] int? eID)
	{
		// Basic validation of inputs
		if (string.IsNullOrWhiteSpace(vc) || eID is null || eID <= 0)
		{
			TempData["EmailVerificationSuccess"] = false;
			TempData["EmailVerificationMessage"] = "Invalid verification link.";
			TempData["EmailVerificationCode"] = "INVALID_LINK";
			return RedirectToAction(nameof(Result));
		}

		var user = HttpContext.User.ToUserApiAccess();
		if (user is null)
		{
			TempData["EmailVerificationSuccess"] = false;
		//TempData["EmailVerificationMessage"] = "You must be logged in to verify your email.";
			TempData["EmailVerificationCode"] = "UNAUTHORIZED";
			return RedirectToAction(nameof(Result));
		}

		// Ensure the link belongs to the logged-in user
		if (user.EntityId != eID)
		{
			TempData["EmailVerificationSuccess"] = false;
			//TempData["EmailVerificationMessage"] = "This verification link doesn't match the logged-in user.";
			TempData["EmailVerificationCode"] = "FORBIDDEN";
			return RedirectToAction(nameof(Result));
		}

		try
		{
			var result = await _userService.VerifyEmailAsync(new VerifyEmailDto { Code = vc }, user);

			if (result.IsSuccess && result.Value)
			{
				// Try to grant email verification bonus
				await TryGrantEmailVerificationBonusAsync(user);

				TempData["EmailVerificationSuccess"] = true;
				TempData["EmailVerificationMessage"] = "Email verified successfully.";
				TempData["EmailVerificationCode"] = string.Empty;
			}
			else
			{
				TempData["EmailVerificationSuccess"] = false;
				TempData["EmailVerificationMessage"] = result.Error?.Message ?? "Failed to verify email.";
				TempData["EmailVerificationCode"] = result.Error?.Code ?? "UNEXPECTED_ERROR";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error during email verification for Entity {EntityId}", user.EntityId);
			TempData["EmailVerificationSuccess"] = false;
			//TempData["EmailVerificationMessage"] = "An unexpected error occurred during email verification.";
			TempData["EmailVerificationCode"] = "UPSTREAM_ERROR";
		}

		return RedirectToAction(nameof(Result));
	}

	// GET /email-verification/result
	[HttpGet("email-verification/result")]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public IActionResult Result()
	{
		var user = HttpContext.User.ToUserApiAccess();
		var model = new EmailVerificationResultViewModel
		{
			Success = bool.TryParse(TempData["EmailVerificationSuccess"]?.ToString(), out var ok) && ok,
			IsAlreadyVerified = TempData["EmailVerificationCode"]?.ToString() == "ALREADY_VERIFIED",
			Message = TempData["EmailVerificationMessage"]?.ToString(),
			ErrorCode = TempData["EmailVerificationCode"]?.ToString(),
			EntityId = user?.EntityId,
			AffiliateId = user?.AffiliateId
		};

		// Render the host app view (placed under old-main-umbraco/Views/EmailVerification/Result.cshtml)
		return View("~/Views/EmailVerification/Result.cshtml", model);
	}

	// GET /email-verification (alias to result page to display any TempData messages)
	[HttpGet("email-verification")]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public async Task<IActionResult> EmailVerification()
	{
		var access = HttpContext.User.ToUserApiAccess();
		if (access is null)
		{
			return Challenge();
		}

		string? email = null;
		bool? emailVerified = null;

		try
		{
			var me = await _userService.GetMeAsync(access);
			if (me.IsSuccess)
			{
				email = me.Value.Item1.Email;
				emailVerified = me.Value.Item1.EmailVerified;

				if (emailVerified == false)
					await _userService.VerifyEmailAsync(new VerifyEmailDto { Code = "" }, access);
				if (emailVerified == true)
				{
					TempData["EmailVerificationSuccess"] = true;
					TempData["EmailVerificationMessage"] = "Your email is already verified";
					TempData["EmailVerificationCode"] = "ALREADY_VERIFIED";
					HttpContext.Response.Headers["Redirect"] = "/email-verification";
					return RedirectToAction(nameof(Result));
				}
			}
			else
			{
				_logger.LogWarning("GetMeAsync failed for EntityId {EntityId}. Code={Code} Message={Message}", access.EntityId, me.Error?.Code, me.Error?.Message);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while loading email verification page for EntityId {EntityId}", access.EntityId);
		}

		var model = new EmailVerificationPageViewModel
		{
			Email = email ?? string.Empty,
			IsEmailVerified = emailVerified
		};

		if (string.IsNullOrEmpty(email))
		{
			return RedirectToAction("Change");
		}

		return View("~/Views/EmailVerification/EmailVerification.cshtml", model);
	}

	// GET /email-verification/change (set a hint and show the same page; UI may link to a profile/email-change section)
	[HttpGet("email-verification/change")]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public IActionResult Change()
	{
		// Optional TempData for consistency (not strictly needed by the view)
		TempData["EmailVerificationSuccess"] = false;
		TempData["EmailVerificationMessage"] = "You can update your email address below.";
		TempData["EmailVerificationCode"] = "CHANGE_REQUEST";
		return View("~/Views/EmailVerification/ChangeEmail.cshtml");
	}

	/// <summary>
	/// Grants email verification bonus if not already granted.
	/// </summary>
	private async Task TryGrantEmailVerificationBonusAsync(GoldCasino.ApiModule.Models.UserApiAccess access)
	{
		try
		{
			// Check if bonus already exists
			var bonusResult = await _businessApiService.EntityBonusesGetAsync(new EntityBonusesGet
			{
				Filter = new Dictionary<string, string>
				{
					["isDeleted"] = "=0",
					["CustomField201"] = $"={(int)BonusType.EmailVerification}"
				},
				LimitFrom = 0,
				LimitCount = 1
			}, access);

			if (!bonusResult.IsSuccess)
			{
				_logger.LogWarning("Failed to check email verification bonus for EntityId {EntityId}: {Error}",
					access.EntityId, bonusResult.Error?.Message ?? bonusResult.Error?.Code);
				return;
			}

			// If bonus already exists, skip
			var existingBonus = bonusResult.Value?.Data?.FirstOrDefault(b =>
				b.Type == BonusType.EmailVerification && (b.IsDeleted ?? 0) == 0);

			if (existingBonus is not null)
			{
				_logger.LogInformation("Email verification bonus already exists for EntityId {EntityId}", access.EntityId);
				return;
			}

			// Add new email verification bonus
			var nowUtc = DateTime.UtcNow;
			var expiryUtc = nowUtc.AddHours(24);

			var updateResult = await _playerClub365ApiService.EntityBonusesUpdateAsync(new EntityBonusesUpdate
			{
				RecordId = 0,
				EntityId = access.EntityId,
				BonusType = BonusType.EmailVerification,
				Serial = 0,
				Value = EmailVerificationBonusValue,
				Used = 0,
				CreatedDateCustom = nowUtc,
				ExpirationDate = expiryUtc
			});

			if (!updateResult.IsSuccess)
			{
				_logger.LogError("Failed to grant email verification bonus for EntityId {EntityId}: {Error}",
					access.EntityId, updateResult.Error?.Message ?? updateResult.Error?.Code);
				return;
			}

			_logger.LogInformation("Email verification bonus of {Value} granted to EntityId {EntityId}",
				EmailVerificationBonusValue, access.EntityId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while granting email verification bonus for EntityId {EntityId}", access.EntityId);
		}
	}
}

public class EmailVerificationResultViewModel
{
	public bool Success { get; set; }
	public bool IsAlreadyVerified { get; set; } = false;
	public string? Message { get; set; }
	public string? ErrorCode { get; set; }
	public int? EntityId { get; set; }
	public int? AffiliateId { get; set; }
}

public class EmailVerificationPageViewModel
{
	public string? Email { get; set; }
	public bool? IsEmailVerified { get; set; }
}